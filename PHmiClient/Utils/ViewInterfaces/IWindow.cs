using System.ComponentModel;
using System.Windows;

namespace PHmiClient.Utils.ViewInterfaces
{
    public interface IWindow
    {
        Rect RestoreBounds { get; }
        event CancelEventHandler Closing;
        double Top { get; set; }
        double Left { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        WindowState WindowState { get; set; }
        WindowStartupLocation WindowStartupLocation { get; set; }
        void Close();
        bool? DialogResult { get; set; }
        bool? ShowDialog();
        string Title { get; set; }
    }
}
