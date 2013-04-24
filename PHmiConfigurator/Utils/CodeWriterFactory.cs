using System.IO;
using System.Text;

namespace PHmiConfigurator.Utils
{
    public class CodeWriterFactory : ICodeWriterFactory
    {
        public ICodeWriter Create(string file)
        {
            return new CodeWriter(new StreamWriter(File.Create(file), Encoding.UTF8) { AutoFlush = true });
        }
    }
}
