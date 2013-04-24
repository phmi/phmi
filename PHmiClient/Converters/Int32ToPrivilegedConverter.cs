using System;
using System.Globalization;

namespace PHmiClient.Converters
{
    public class Int32ToPrivilegedConverter : ChainConverter
    {
        public Int32ToPrivilegedConverter()
        {
            AllowNull = true;
        }

        public static string Convert(int? value)
        {
            if (value == null)
                return null;
            var intValue = (int)value;
            var privilege = string.Empty;
            var mask = 1;
            for (var i = 1; i <= 32; i++)
            {
                if ((intValue & mask) != 0)
                {
                    if (string.IsNullOrEmpty(privilege))
                        privilege = i.ToString(CultureInfo.InvariantCulture);
                    else
                        privilege += ", " + i;
                }
                mask = mask << 1;
            }
            return privilege;
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var intValueN = value as int?;
            return Convert(intValueN);
        }

        public static int? ConvertBack(string value)
        {
            if (value == null)
                return null;

            uint? privilege = null;
            var mask = 0x80000000;
            for (var i = 32; i > 0; i--)
            {
                if (value.Contains(i.ToString(CultureInfo.InvariantCulture)))
                {
                    value = value.Replace(i.ToString(CultureInfo.InvariantCulture), string.Empty);
                    if (privilege == null)
                        privilege = mask;
                    else
                        privilege = privilege | mask;
                }
                mask = mask >> 1;
            }
            return (int?)privilege;
        }

        public bool AllowNull { get; set; }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strValue = value as string;
            return ConvertBack(strValue);
        }
    }
}