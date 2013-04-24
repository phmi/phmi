using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PHmiClient.Converters.MultiValueConverters
{
    public class NotPrivilegedToCollapsedConverter : IMultiValueConverter
    {
        private readonly IMultiValueConverter _privelegedToTrueConverter = new PrivilegedToTrueConverter();
        
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return _privelegedToTrueConverter.Convert(values, targetType, parameter, culture) as bool? == true
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
