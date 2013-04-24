using PHmiClient.Utils;
using System;
using System.Globalization;

namespace PHmiClient.Converters
{
    public class ByteArrayToImageSourceConverter : ChainConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ImageHelper.ToImage(value as byte[]);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
