using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PHmiClient.Utils
{
    public static class ByteConverter
    {
        public static string BytesToString(IEnumerable<byte> bytes)
        {
            if (bytes == null)
                return null;
            return string.Join(
                string.Empty,
                bytes.Select(
                b =>
                {
                    var s = b.ToString("x", CultureInfo.InvariantCulture);
                    return s.Length == 1 ? string.Format("0{0}", s) : s;
                }));
        }

        public static byte[] StringToBytes(string str)
        {
            if (str == null)
                return null;
            var bytes = new byte[str.Length / 2];
            for (var i = 0; i < str.Length / 2; i++)
            {
                var index = i*2;
                bytes[i] = byte.Parse(
                    string.Format("{0}{1}", str[index], str[index + 1]),
                    NumberStyles.AllowHexSpecifier);
            }
            return bytes;
        }
    }
}
