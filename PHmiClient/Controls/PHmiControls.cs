using System.Windows;

namespace PHmiClient.Controls
{
    public class PHmiControls
    {
        #region Privilege

        public static readonly DependencyProperty PrivilegeProperty = DependencyProperty.RegisterAttached(
            "Privilege", typeof(string), typeof(PHmiControls), new FrameworkPropertyMetadata(null) { Inherits = true });

        public static string GetPrivilege(DependencyObject obj)
        {
            return (string)obj.GetValue(PrivilegeProperty);
        }

        public static void SetPrivilege(DependencyObject obj, string value)
        {
            obj.SetValue(PrivilegeProperty, value);
        }

        #endregion
    }
}
