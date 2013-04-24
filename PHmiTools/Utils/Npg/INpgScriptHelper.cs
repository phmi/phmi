using System.IO;

namespace PHmiTools.Utils.Npg
{
    public interface INpgScriptHelper
    {
        string[] ExtractScriptLines(string script);
        string[] ExtractScriptLines(Stream stream);
    }
}
