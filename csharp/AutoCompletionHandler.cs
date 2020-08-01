using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace csharp
{
    class AutoCompletionHandler : IAutoCompleteHandler
    {
        private ScriptRunner script;

        public char[] Separators { get; set; } = new char[] { ' ', '.', ';', '(', '[', '{' };

        public AutoCompletionHandler(ScriptRunner script)
        {
            this.script = script;
        }

        // text - The current text entered in the console
        // index - The index of the terminal cursor within {text}
        public string[] GetSuggestions(string text, int index)
        {

            string context = GetContext(text);
            List<string> types = GetTypes(context);

            if (types.Count > 0)
            {
                string first = types.FirstOrDefault();
                bool withStatic = true;
                types.RemoveAt(0);

                Type currentType = null;

                var variable = script.Variables.FirstOrDefault(x => x.Name == first);
                if (variable != null)
                {
                    currentType = variable.Type;
                    withStatic = false;
                }

                // there was only 1 type
                if (types.Count == 0)
                {
                    return script.LoadedTypes.Where(x => x.Name.StartsWith(first, StringComparison.OrdinalIgnoreCase)).Select(x => Utilities.GetFullTypeName(x)).Distinct().ToArray();
                }

                // there were only 2 types
                else if (types.Count == 1)
                {
                    currentType ??= script.LoadedTypes.FirstOrDefault(x => x.Name == first);
                    return currentType?.GetSuggestableMemberInfos(withStatic, script).Where(x => x.Name.StartsWith(types.LastOrDefault(), StringComparison.OrdinalIgnoreCase)).Select(x => x.Name).ToArray();
                }

                // there were more than 2 types
                else if (types.Count > 1)
                {
                    string last = types.LastOrDefault();
                    types.RemoveAt(types.Count - 1);
                    currentType ??= script.LoadedTypes.FirstOrDefault(x => x.Name == first);
                    while (types.Count > 0)
                    {
                        currentType = currentType?.GetSuggestableMemberInfos(withStatic, script).FirstOrDefault(x => x.Name == types.FirstOrDefault())?.GetUnderlyingType();
                        types.RemoveAt(0);

                        if (withStatic)
                        {
                            withStatic = false;
                        }

                    }
                    return currentType?.GetSuggestableMemberInfos(withStatic, script).Where(x => x.Name.StartsWith(last, StringComparison.OrdinalIgnoreCase)).Select(x => x.Name).ToArray();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return script.Variables.Select(x => x.Name).Union(script.LoadedTypes.Select(x => Utilities.GetFullTypeName(x))).Distinct().ToArray();
            }
        }

        private static string GetContext(string text)
        {
            string context = text;
            int count = 0;

            for (int i = context.Length - 1; i >= 0; i--)
            {
                if (context[i] == ')')
                {
                    count += 1;
                }


                else if (context[i] == '(')
                {
                    if (count > 0)
                    {
                        count -= 1;
                    }
                    else return i == context.Length - 1 ? "" : context[(i + 1)..];
                }
            }


            return context;
        }

        private static List<string> GetTypes(string text)
        {
            List<string> result = new List<string>();
            StringBuilder word = new StringBuilder();
            int count = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '(')
                {
                    count += 1;
                    if (count == 0)
                    {
                        result.Add(word.ToString().Trim());
                        word.Clear();
                    }
                }

                else if (text[i] == ')')
                {
                    if (count > 0)
                    {
                        count -= 1;
                    }
                    else return new List<string>();
                }
                else if (text[i] == '?')
                {
                    continue;
                }
                else if (count == 0)
                {
                    if (text[i] == '.')
                    {
                        if (word.Length > 0)
                        {
                            result.Add(word.ToString().Trim());
                            word.Clear();
                        }
                    }
                    else
                    {
                        word.Append(text[i]);
                    }
                }
            }

            if (word.Length > 0)
            {
                result.Add(word.ToString().Trim());
            }

            if (text.LastOrDefault() == '.')
            {
                result.Add("");
            }

            return result;
        }
    }
}
