using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PHmiClient.Controls.Trends
{
    public class SliderLineVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = (bool) values[0];
            var cursorCoordinateNotNull = values[1] != null;
            if (visible && cursorCoordinateNotNull)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
