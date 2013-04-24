using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace PHmiClient.Converters
{
    public class PasswordConverter : ChainConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strVal = value as string;
            return strVal == null ? null : "*";
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strVal = value as string;
            return ConvertBack(strVal);
        }

        public static string ConvertBack(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            using (var cripto = SHA512.Create())
            {
                var bytes = cripto.ComputeHash((new UTF8Encoding()).GetBytes(value + "phmicojib"));
                var psw = new StringBuilder();
                foreach (var b in bytes)
                {
                    psw.Append(b.ToString(CultureInfo.InvariantCulture));
                }
                return psw.ToString();
            }
        }
    }
}
