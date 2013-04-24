using PHmiClient.Logs;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PHmiClientSample.Converters
{
    public class LogItemToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bytes = value as LogItem;
            if (bytes == null)
                return null;
            return bytes.GetFromBytes();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
