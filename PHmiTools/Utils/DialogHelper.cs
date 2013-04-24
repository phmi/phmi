using System;
using System.Windows;
using PHmiTools.Dialogs;

namespace PHmiTools.Utils
{
    public class DialogHelper : IDialogHelper
    {
        public bool Exception(Exception exception, object owner = null)
        {
            return ExceptionDialog.Show(exception, owner);
        }

        public bool? Message(
            string message,
            string header,
            MessageBoxButton button = MessageBoxButton.OK,
            MessageBoxImage image = MessageBoxImage.Asterisk,
            object owner = null)
        {
            return MessageDialog.Show(message, header, button, image, owner);
        }
    }
}
