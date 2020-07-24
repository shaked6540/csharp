// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "module")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "module")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "This method needs to be not static", Scope = "member", Target = "~M:cs.Globals.help(System.Type)")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Code is never null", Scope = "member", Target = "~M:csharp.ScriptRunner.ContinueWithAsync(System.String)~System.Threading.Tasks.Task")]
