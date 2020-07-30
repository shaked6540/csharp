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

        public char[] Separators { get; set; } = new char[] { ' ', '.', ';' };

        public AutoCompletionHandler(ScriptRunner script)
        {
            this.script = script;
        }

        // text - The current text entered in the console
        // index - The index of the terminal cursor within {text}
        public string[] GetSuggestions(string text, int index)
        {
            var statements = GetStatements(text);

            if (statements.Count > 0)
            {
                string first = statements.FirstOrDefault();
                bool withStatic = true;
                statements.RemoveAt(0);

                Type currentType = null;

                var variable = script.Variables.FirstOrDefault(x => x.Name == first);
                if (variable != null)
                {
                    currentType = variable.Type;
                    withStatic = false;
                }

                // there was only 1 statement
                if (statements.Count == 0)
                {
                    return script.LoadedTypes.Where(x => x.Name.StartsWith(first, StringComparison.OrdinalIgnoreCase)).Select(x => Utilities.GetFullTypeName(x)).Distinct().ToArray();
                }

                // there were only 2 statements
                else if (statements.Count == 1)
                {
                    currentType ??= script.LoadedTypes.FirstOrDefault(x => x.Name == first);
                    return currentType?.GetSuggestableMemberInfos(withStatic, script).Where(x => x.Name.StartsWith(statements.LastOrDefault(), StringComparison.OrdinalIgnoreCase)).Select(x => x.Name).ToArray();
                }

                // there were more than 2 statements
                else if (statements.Count > 1)
                {
                    string last = statements.LastOrDefault();
                    statements.RemoveAt(statements.Count - 1);
                    currentType ??= script.LoadedTypes.FirstOrDefault(x => x.Name == first);
                    while (statements.Count > 0)
                    {
                        currentType = currentType?.GetSuggestableMemberInfos(withStatic, script).FirstOrDefault(x => x.Name == statements.FirstOrDefault())?.GetUnderlyingType();
                        statements.RemoveAt(0);

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

        private static List<string> GetStatements(string text)
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
                        result.Add(word.ToString());
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

                else if (count == 0)
                {
                    if (text[i] == '.')
                    {
                        if (word.Length > 0)
                        {
                            result.Add(word.ToString());
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
                result.Add(word.ToString());
            }

            if (text.LastOrDefault() == '.')
            {
                result.Add("");
            }

            return result;
        }
    }
}
