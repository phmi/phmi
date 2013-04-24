using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PHmiTools.Utils.Npg
{
    public class NpgScriptHelper : INpgScriptHelper
    {
        public string[] ExtractScriptLines(string script)
        {
            var uncommentRegex = new Regex(@"/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/");
            var uncommented = uncommentRegex.Replace(script, string.Empty);
            var trimmRegex = new Regex(@"[\s]+");
            var trimmed = trimmRegex.Replace(uncommented, " ");
            var splited = trimmed
                .Split(';')
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => s.Trim()).ToArray();
            var result = new List<string>(splited.Length);
            var opened = false;
            var row = string.Empty;
            foreach (var s in splited)
            {
                row += s + ";";
                var count = s.Count(ch => ch == '\'');
                if (count % 2 != 0)
                    opened = !opened;
                if (!opened)
                {
                    result.Add(row);
                    row = string.Empty;
                }
            }
            return result.ToArray();
        }

        public string[] ExtractScriptLines(Stream stream)
        {
            using (stream)
            {
                var streamReader = new StreamReader(stream, Encoding.UTF8);
                return ExtractScriptLines(streamReader.ReadToEnd());
            }
        } 
    }
}
