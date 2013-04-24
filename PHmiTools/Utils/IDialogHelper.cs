using System;
using System.Windows;

namespace PHmiTools.Utils
{
    public interface IDialogHelper
    {
        bool Exception(Exception exception, object owner = null);

        bool? Message(
            string message,
            string header,
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage image = MessageBoxImage.Asterisk,
            object owner = null);
    }
}
