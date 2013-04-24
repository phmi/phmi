using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace PHmiClient.Utils
{
    public static class WindowExtentions
    {
        public static void UpdateLanguage(this Window window)
        {
            window.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentUICulture.IetfLanguageTag);
        }

        public static bool? ShowDialog(this Window window, Control owner)
        {
            window.Owner = Window.GetWindow(owner);
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            return window.ShowDialog();
        }
    }
}
