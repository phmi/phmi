using System;
using System.Globalization;

namespace PHmiClient.Converters
{
    public class UniversalToLocalTimeConverter : ChainConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var utc = value as DateTime?;
            if (utc.HasValue)
                return utc.Value.ToLocalTime();
            return null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var local = value as DateTime?;
            if (local.HasValue)
                return local.Value.ToUniversalTime();
            return null;
        }
    }
}
