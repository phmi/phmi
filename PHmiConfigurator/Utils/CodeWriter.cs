using System;
using System.IO;

namespace PHmiConfigurator.Utils
{
    public class CodeWriter : ICodeWriter
    {
        private readonly TextWriter _writer;
        private int _offset;
        
        public CodeWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public void Write(string line = null, params object[] args)
        {
            if (line == "}}")
                _offset--;

            var s = string.Empty;
            for (var i = 0; i < _offset; i++)
            {
                s += "\t";
            }
            var indentedLine = s + line;
            _writer.WriteLine(indentedLine, args);

            if (line == "{{")
                _offset++;
        }

        public IDisposable Block(string line = null, params object[] args)
        {
            return new CodeBlock(this, line, args);
        }

        private class CodeBlock : IDisposable
        {
            private readonly ICodeWriter _writer;

            public CodeBlock(ICodeWriter writer, string line, params object[] parameters)
            {
                _writer = writer;
                _writer.Write(line, parameters);
                _writer.Write("{{");
            }

            public void Dispose()
            {
                _writer.Write("}}");
            }
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
