using System;
using System.Globalization;

namespace PHmiClient.Converters
{
    public class IntToStringConverter: ChainConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var i = value as int?;
            if (i.HasValue)
            {
                return i.ToString();
            }
            return null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strVal = value as string;
            if (string.IsNullOrEmpty(strVal))
                return 0;
            int i;
            if (int.TryParse(strVal, out i))
            {                
                return i;
            }
            return 0;
        }
    }
}
