using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace csharp
{
    public static class Utilities
    {
        // Assembly.GetEntryAssembly().Location doesn't work in .net core's single file app, so we use this workaround
        public static readonly string ExecuteablePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "/";
        public static readonly string ScriptFolder = ExecuteablePath + "Scripts";
        public static readonly string ConfigPath = ExecuteablePath + "Config.json";
        public static readonly JsonSerializerSettings settings = new JsonSerializerSettings() { ContractResolver = new MyContractResolver() };
        public static void WriteInColor(object value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ResetColor();
        }
        public static int NthIndexOf(this string s, string match, int occurence)
        {
            int i = 1;
            int index = 0;

            while (i <= occurence && (index = s.IndexOf(match, index + 1, StringComparison.Ordinal)) != -1)
            {
                if (i == occurence)
                    return index;

                i++;
            }

            return -1;
        }
        public static void GetFullTypeName(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t));
            }

            if (t.GetGenericArguments().Length <= 0)
            {
                WriteInColor(t.Name, t.IsPrimitive ? ConsoleColor.Blue : ConsoleColor.DarkGreen);
                return;
            }

            WriteInColor(GetNameWithoutGenericArity(t), t.IsPrimitive ? ConsoleColor.Blue : ConsoleColor.DarkGreen);
            Console.Write("<");
            var arguments = t.GetGenericArguments();
            for (int i = 0; i < arguments.Length; i++)
            {
                GetFullTypeName(arguments[i]);

                if (i != arguments.Length - 1)
                    Console.Write(", ");
            }
            Console.Write(">");
        }

        public static string GetNameWithoutGenericArity(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t));
            }
            string name = t.Name;
            int index = name.IndexOf('`', StringComparison.Ordinal);
            return index == -1 ? name : name.Substring(0, index);
        }

        internal class MyContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var props = type.GetProperties()
                    .Where(x => !x.IsSpecialName)
                    .Select(p => base.CreateProperty(p, memberSerialization))
                    .Union(type.GetFields().Where(x => !x.IsStatic && !x.IsSpecialName).Select(f => base.CreateProperty(f, memberSerialization)))
                    .ToList();
                props.ForEach(p => { p.Writable = true; p.Readable = true; });
                return props;
            }
        }
    }
}
