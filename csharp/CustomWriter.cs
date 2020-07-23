using System.Text;
using System.IO;

namespace csharp
{
    class CustomWriter: TextWriter
    {
        private TextWriter console, file;

        public override Encoding Encoding => console.Encoding;

        public CustomWriter(TextWriter console, TextWriter file)
        {
            this.console = console;
            this.file = file;
        }

        public override void Write(char value)
        {
            file.Write(value);
            file.Flush();
            console.Write(value);
            console.Flush();
        }
    }
}
