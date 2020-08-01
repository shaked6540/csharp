using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using MoreLinq;
using Microsoft.CodeAnalysis.Scripting;

namespace csharp
{
    public static class Utilities
    {
        // Assembly.GetEntryAssembly().Location doesn't work in .net core's single file app, so we use this workaround
        public static readonly string ExecuteablePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "/";
        public static readonly string ScriptFolder = ExecuteablePath + "Scripts";
        public static readonly string ConfigPath = ExecuteablePath + "Config.json";
        public static readonly JsonSerializerSettings settings = new JsonSerializerSettings() { ContractResolver = new MyContractResolver(), Formatting = Formatting.Indented };
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
        public static void PrintFullTypeName(Type t)
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
                PrintFullTypeName(arguments[i]);

                if (i != arguments.Length - 1)
                    Console.Write(", ");
            }
            Console.Write(">");
        }

        public static string GetFullTypeName(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t));
            }

            if (t.GetGenericArguments().Length <= 0)
            {
                return t.Name;
            }

            string nameWithoutGenericArity = GetNameWithoutGenericArity(t);
            nameWithoutGenericArity += "<";
            var arguments = t.GetGenericArguments();
            for (int i = 0; i < arguments.Length; i++)
            {
                nameWithoutGenericArity += GetFullTypeName(arguments[i]);

                if (i != arguments.Length - 1)
                    nameWithoutGenericArity += ", ";
            }
            nameWithoutGenericArity += ">";
            return nameWithoutGenericArity;
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

        public static Type GetUnderlyingType(this MemberInfo member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }

        public static List<MemberInfo> GetSuggestableMemberInfos(this Type type, bool isStatic, ScriptRunner script)
        {
            if (type == null || script == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var members = new List<MemberInfo>();
            var properties = type.GetProperties().DistinctBy(x => x.Name);
            var fields = type.GetFields().Where(x=> !x.IsSpecialName).DistinctBy(x => x.Name);
           
            var extensionMethods = script.ExtensionMethods.Where(x=> x.Value.Any(v=> v.Item2.IsAssignableFrom(type) || 
            type.GetTypeInfo().ImplementedInterfaces.Any(typeInfo=> GetNameWithoutGenericArity(typeInfo) == x.Key)))
                .SelectMany(x=> x.Value.Select(v=> v.Item1));
            var methods = type.GetMethods().Where(x=> !x.IsSpecialName);
            var events = type.GetEvents().DistinctBy(x => x.Name);

            if (!isStatic)
            {
                properties = properties.Where(x => !x.IsStatic());
                fields = fields.Where(x => !x.IsStatic);
                methods = methods.Where(x => !x.IsStatic);
                events = events.Where(x => !x.IsStatic());
            }

            if (extensionMethods != null && !isStatic)
            {
                methods = methods.Union(extensionMethods).DistinctBy(x => x.Name);
            }
            else
            {
                methods = methods.DistinctBy(x => x.Name);
            }

            members.AddRange(properties);
            members.AddRange(fields);
            members.AddRange(methods);
            members.AddRange(events);

            return members.DistinctBy(x=> x.Name).OrderBy(x=> x.Name).ToList();
        }

        public static TValue AddOrGet<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
        {
            TValue val;
            
            if (!dict.TryGetValue(key, out val))
            {
                val = new TValue();
                dict.Add(key, val);
            }

            return val;
        }

        public static bool IsStatic(this MemberInfo member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }

            switch (member.MemberType)
            {

                case MemberTypes.Field:
                    return ((FieldInfo)member).IsStatic;
                case MemberTypes.Method:
                    return ((MethodInfo)member).IsStatic;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).GetAccessors(true)[0].IsStatic;
                case MemberTypes.Constructor:
                    return ((ConstructorInfo)member).IsStatic;
                case MemberTypes.Event:
                    return ((EventInfo)member).GetAddMethod().IsStatic;

                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }

        public static List<Assembly> GetAssemblies()
        {
            var result = new List<Assembly>();
            var assemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies();

            foreach (var a in assemblies)
                result.Add(Assembly.Load(a));
             
            result.Add(Assembly.GetEntryAssembly());

            var files =Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "*.dll");
            foreach (var file in files)
            {
                result.Add(Assembly.LoadFile(file));
            }

            return result;
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
