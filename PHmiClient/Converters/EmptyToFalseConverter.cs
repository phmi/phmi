using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace PHmiClient.Converters
{
    public class EmptyToFalseConverter : ChainConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                return enumerable.Cast<object>().Any();
            }
            return value != null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
