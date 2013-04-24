using System.Windows;
using System.Windows.Controls;
using System;
using PHmiIoDevice.Generic.Loc;
using PHmiIoDeviceTools;

namespace PHmiIoDevice.Generic
{
    public class GenericOptionsEditor : TextBox, IOptionsEditor
    {
        public void SetOptions(string options)
        {
            IsReadOnly = true;
            BorderThickness = new Thickness(0);
            Text = Res.NoOptionsRequired;
            Loaded += OptionsEditorLoaded;
        }

        private void OptionsEditorLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var changed = OptionsChanged;
            if (changed != null)
            {
                changed(this, EventArgs.Empty);
            }
        }

        public string GetOptions()
        {
            return string.Empty;
        }

        public event EventHandler OptionsChanged;
    }
}
