using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PHmiClient.Controls.ListBoxWithColumns
{
    public class ReverseDoubleToThicknesConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value as double?;
            if (val != null)
                return new Thickness(-val.Value, 0, 0, 0);
            return new Thickness();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
