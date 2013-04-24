using System;
using System.Globalization;
using System.Windows;

namespace PHmiClient.Converters
{
    public class FalseToCollapsedConverter : ChainConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as bool? == true ? Visibility.Visible : Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as Visibility? == Visibility.Visible;
        }
    }
}
