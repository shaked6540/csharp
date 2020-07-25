using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json; 
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace csharp
{
    using static Utilities;
    public class ScriptRunner
    {
        private ScriptState script;
        private readonly Config config;
        public ImmutableArray<ScriptVariable> Variables => script.Variables;

        public static async Task<ScriptRunner> GetScriptRunner()
        {
            Config config = Config.DefaultConfig;
            
            // parse config from the config file if it exists
            if (File.Exists(ConfigPath))
            {
                try
                {
                    config = JsonConvert.DeserializeObject<Config>(await File.ReadAllTextAsync(ConfigPath).ConfigureAwait(false));
                }
                catch (Exception ex)
                {
                    WriteInColor($"Could not load config file: {ex.Message}\n", ConsoleColor.Red);
                }
            }

            var scriptRunner = new ScriptRunner(config);

            string beforeScriptCode = null;
            if (config.BeforeScriptFile != null && File.Exists(config.BeforeScriptFile))
            {
                try
                {
                    beforeScriptCode = await File.ReadAllTextAsync(config.BeforeScriptFile).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    WriteInColor($"Could not load before script file: {ex.Message}\n", ConsoleColor.Red);
                }
            }

            await scriptRunner.CreateScriptAsync("1+1", new Globals(scriptRunner)).ConfigureAwait(false);

            if (beforeScriptCode != null)
            {
                await scriptRunner.ContinueWithAsync(beforeScriptCode).ConfigureAwait(false);
            }

            return scriptRunner;
        }

        
        private ScriptRunner(Config config)
        {
            this.config = config;

            if (config.SaveScripts)
            {
                Directory.CreateDirectory(config.SavedScriptsLocation);

                // find all the saved script files and get the latest script number
                var files = Directory.GetFiles(config.SavedScriptsLocation, "Script*.cs");
                var latestScripNumber = 1;
                if (files.Any())
                {
                    // find the max number of the scripts and increment it by 1
                    var pathLength = config.SavedScriptsLocation.Length + "/Script ".Length;
                    latestScripNumber = files.Max(x => int.Parse(x[pathLength..(x.Length - 3)], NumberStyles.None, CultureInfo.InvariantCulture)) + 1;
                }

                // create the script file and pass the stream writer to the custom reader/writer
                var sw = File.CreateText($"{config.SavedScriptsLocation}/Script {latestScripNumber}.cs");
                Console.SetOut(new CustomWriter(Console.Out, sw));
                Console.SetIn(new CustomReader(Console.In, sw));
            }
        }

        public async Task ContinueWithAsync(string code)
        {
            runScript:
            try
            {
                script = await script.ContinueWithAsync(code).ConfigureAwait(false);

                if (script.ReturnValue != null)
                {
                    GetFullTypeName(script.ReturnValue.GetType());
                    Console.WriteLine($" {JsonConvert.SerializeObject(script.ReturnValue, settings)}");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains(": error CS1002: ; expected", StringComparison.Ordinal))
                {
                    // find the place where the semicolon is missing using the exception details
                    var sub = ex.Message.Substring(0, ex.Message.IndexOf(':', StringComparison.Ordinal));
                    var index = sub.IndexOf(',', StringComparison.Ordinal) + 1;
                    var row = sub[1..(index - 1)];
                    var col = sub[index..^1];
                    if (int.TryParse(row, out var rowResult) && int.TryParse(col, out var colResult))
                    {
                        // since the number of rows doesnt indicate length, we need to get to the row-nth index of '\n'
                        // in the code to determine in what line the semicolon is missing, and add the col number to that
                        // to get the index of where the semicolon is missing in the code string
                        var startIndex = code.NthIndexOf("\n", rowResult - 1) + colResult;
                        code = code.Insert(startIndex, ";");
                        goto runScript;
                    }
                }

                if (ex.Message.Contains(": error CS1513: } expected", StringComparison.Ordinal))
                {
                    code = ReadUntilCurlyBracketsNumberMatch(code);
                    goto runScript;
                }

                if (ex.Message.Contains(": error CS1733: Expected expression", StringComparison.Ordinal) ||
                    ex.Message.Contains(": error CS1514: { expected", StringComparison.Ordinal))
                {
                    Console.WriteLine(Config.OpenCurlyBracket);
                    code += $"\n{Config.OpenCurlyBracket}\n";
                    code = ReadUntilCurlyBracketsNumberMatch(code);
                    goto runScript;
                }

                throw new Exception(ex.Message, ex);
            }
        }

        public async Task ReadAndExectueAsync()
        {
            string code;
            WriteInColor(config.NewlinePrefix, ConsoleColor.White);
            while ((code = Console.ReadLine()) != config.EndKeyword)
            {
                // if the multi line keyword was entered, read lines until the end keyword is entered
                if (code == config.MultilineCodeKeyword)
                {
                    string multiLineCode = string.Empty, singleLine;
                    while ((singleLine = Console.ReadLine()) != config.EndKeyword)
                    {
                        multiLineCode += string.Concat(singleLine, "\n");
                    }
                    code = multiLineCode;
                }

                try
                {
                    await ContinueWithAsync(code).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(ex.Message);
                    Console.ResetColor();
                }
                WriteInColor(config.NewlinePrefix, ConsoleColor.White);
            }
        }

        private static string ReadUntilCurlyBracketsNumberMatch(string initalCode)
        {
            string code = initalCode;
            while (code.Count(x => x.Equals(Config.OpenCurlyBracket)) != code.Count(x => x.Equals(Config.CloseCurlyBracket)))
            {
                Console.Write(" ");
                code += string.Concat(Console.ReadLine(), "\n");
            }
            return code;
        }

        private async Task CreateScriptAsync<T>(string code, T globals)
        {
            static IEnumerable<Assembly> GetAssemblies()
            {
                var Assemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();
                foreach (var a in Assemblies)
                {
                    var asm = Assembly.Load(a);
                    yield return asm;
                }
                yield return Assembly.GetEntryAssembly();
                yield return typeof(ILookup<string, string>).GetTypeInfo().Assembly;
            }

            var namespaces = new List<string>()
            {
                "System", "System.Collections", "System.Collections.Generic", "System.Diagnostics","System.IO", "System.Linq", "System.Math",
                "System.Reflection", "System.Runtime", "System.Threading.Tasks", "System.Text",
                "System.Diagnostics", "System.IO", "System.Text.RegularExpressions", "Newtonsoft.Json"
            };
            namespaces.AddRange(config.Namespaces);

            var userAssemblies= new List<Assembly>();


            foreach (var dll in config.Assemblies)
            {
                try
                {
                    userAssemblies.Add(Assembly.LoadFrom($"{ExecuteablePath}/{dll}"));
                }
                catch (Exception ex)
                {
                    WriteInColor($"{ex.Message}\n", ConsoleColor.Red);
                }
                
            }
            userAssemblies.AddRange(GetAssemblies());

            var options = ScriptOptions.Default
                .AddReferences(userAssemblies)
                .AddImports(namespaces)
                .WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest)
                .WithOptimizationLevel(Microsoft.CodeAnalysis.OptimizationLevel.Release);

            script = await CSharpScript.RunAsync(code, options, globals, typeof(T)).ConfigureAwait(false);
        }
    }
}
