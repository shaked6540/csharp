using System.IO;

namespace csharp
{
    class CustomReader : TextReader
    {
        private TextReader reader;
        private TextWriter file;

        public CustomReader(TextReader reader, TextWriter file)
        {
            this.reader = reader;
            this.file = file;
        }

        public override string ReadLine()
        {
            var result = reader.ReadLine();
            file.WriteLine(result);
            return result;
        }
    }
}
