using System;
using System.Collections.Generic;

namespace csharp
{
    public class Config
    {
        public IEnumerable<string> Assemblies { get; set; } = Array.Empty<string>();
        public IEnumerable<string> Namespaces { get; set; } = Array.Empty<string>();
        public string BeforeScriptFile { get; set; } = null;
        public bool SaveScripts { get; set; } = false;
        public string SavedScriptsLocation { get; set; } = Utilities.ScriptFolder;
        public string NewlinePrefix { get; set; } = "> ";
        public string EndKeyword { get; set; } = "end";
        public string MultilineCodeKeyword { get; set; } = "lines";

        public const char OpenCurlyBracket = '{';
        public const char CloseCurlyBracket = '}';

        public static Config DefaultConfig { get; } = new Config();

    }
}
