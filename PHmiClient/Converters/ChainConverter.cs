using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace PHmiClient.Converters
{
    [ContentProperty("Converter")]
    public abstract class ChainConverter : IValueConverter
    {
        public IValueConverter Converter { get; set; }

        public MultiBinding b { get; set; }

        public virtual Type ThisType { get { return null; } }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Converter != null)
                value = Converter.Convert(value, ThisType ?? targetType, parameter, culture);
            return Convert(value, targetType, parameter, culture);
        }

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }
}
