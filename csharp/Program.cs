using System;
using System.Threading.Tasks;

namespace csharp
{
    public static class Program
    {

        static async Task Main(string[] args)
        {
            // initalize the script runner
            var script = await ScriptRunner.GetScriptRunner().ConfigureAwait(false);
            ReadLine.AutoCompletionHandler = new AutoCompletionHandler(script);
            ReadLine.HistoryEnabled = true;

            // treat arguments as code, join all argument by spaces and execute them
            if (args.Length > 0)
            {
                await script.ContinueWithAsync(string.Join(" ", args)).ConfigureAwait(false);
            }

            // main code loop
            await script.ReadAndExectueAsync().ConfigureAwait(false);
        }
    }
}