using PHmiClient.Users;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PHmiClient.Converters.MultiValueConverters
{
    public class PrivilegedToTrueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var user = values[1] as User;
            var privilegeStr = values[0] as string;
            if (string.IsNullOrEmpty(privilegeStr))
                return true;
            var privilege = Int32ToPrivilegedConverter.ConvertBack(privilegeStr);
            if (!privilege.HasValue || privilege.Value == 0)
                return true;
            if (user == null || !user.Privilege.HasValue)
                return false;
            return (user.Privilege.Value & privilege.Value) != 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
