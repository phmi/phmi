using System;
using System.Globalization;
using System.Windows.Data;

namespace PHmiClient.Controls.Trends
{
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var d = value as double?;
            if (d.HasValue)
            {
                return d.Value.ToString(CultureInfo.CurrentCulture);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strVal = value as string;
            if (string.IsNullOrEmpty(strVal))
                return null;
            double d;
            if (double.TryParse(strVal, out d))
                return d;
            return null;
        }
    }
}
