using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace csharp
{
    using static Utilities;
    public class Globals
    {
        private readonly ScriptRunner script;
        public Globals(ScriptRunner script)
        {
            this.script = script;
        }

        /// <summary>
        /// Prints all the variables and their values that were defined in the script
        /// </summary>
        public void variables()
        {
            var variables = script.Variables;
            for (int i = 0; i < variables.Length; i++)
            {
                PrintFullTypeName(variables[i].Type);
                Console.Write($" {variables[i].Name}");
                Console.Write(" = ");
                try
                {
                    Console.WriteLine(JsonConvert.SerializeObject(variables[i].Value, settings));
                }
                catch (Exception ex)
                {
                    WriteInColor(ex.Message, ConsoleColor.Red);
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Prints all the methods, properties, fields and events of a given object
        /// </summary>
        public void help(object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            help(o.GetType());       
        }

        /// <summary>
        /// Prints all the methods, properties, fields and events of a given Type
        /// </summary>
        public void help(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            foreach (var field in type.GetFields())
            {
                WriteInColor($"{(field.IsPublic ? "public " : "private ")}{(field.IsStatic ? "static " : "")}", ConsoleColor.Blue);
                PrintFullTypeName(field.FieldType);
                Console.WriteLine($" {field.Name}");
            }

            foreach (var prop in type.GetProperties())
            {

                var get = prop.GetGetMethod();
                var set = prop.GetSetMethod();

                PrintFullTypeName(prop.PropertyType);
                Console.Write($" {prop.Name} {{ ");

                if (get != null)
                {
                    WriteInColor($"{(get.IsPublic ? "public get; " : "private get; ")}", ConsoleColor.Blue);
                }

                if (set != null)
                {
                    WriteInColor($"{(set.IsPublic ? "public set; " : "private set; ")}", ConsoleColor.Blue);
                }

                Console.WriteLine("}");

            }

            foreach (var method in type.GetMethods().Where(x => !x.IsSpecialName))
            {
                WriteInColor($"{(method.IsPublic ? "public " : "private ")}{(method.IsStatic ? "static " : "")}", ConsoleColor.Blue);
                PrintFullTypeName(method.ReturnType);
                Console.Write($" {method.Name}(");
                var parameters = method.GetParameters();
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].IsOut)
                        WriteInColor("out ", ConsoleColor.Blue);

                    PrintFullTypeName(parameters[i].ParameterType);
                    Console.Write($" {parameters[i].Name}");

                    if (parameters[i].HasDefaultValue)
                        Console.Write($" = {parameters[i].DefaultValue}");

                    if (i != parameters.Length - 1)
                        Console.Write(", ");

                }
                Console.WriteLine(")");
            }


            foreach (var ev in type.GetEvents())
            {
                WriteInColor($"public event ", ConsoleColor.Blue);
                PrintFullTypeName(ev.EventHandlerType);
                Console.WriteLine($" {ev.Name}");
            }
        }


    }
}
