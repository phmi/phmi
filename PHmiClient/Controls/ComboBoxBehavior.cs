using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PHmiClient.Controls
{
    public static class ComboBoxBehavior
    {
        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static bool GetCanSetNull(DependencyObject obj)
        {
            return (bool)obj.GetValue(CanSetNullProperty);
        }

        public static void SetCanSetNull(DependencyObject obj, bool value)
        {
            obj.SetValue(CanSetNullProperty, value);
        }

        // Using a DependencyProperty as the backing store for CanSetNull.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanSetNullProperty =
            DependencyProperty.RegisterAttached("CanSetNull", typeof(bool), typeof(ComboBoxBehavior),
            new FrameworkPropertyMetadata(false, OnCanSetNullChanged));

        private static void OnCanSetNullChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            var comboBox = source as ComboBox;
            if (comboBox == null)
                return;
            if ((bool)args.NewValue)
            {
                comboBox.KeyUp += CbKeyUp;
            }
            else
            {
                comboBox.KeyUp -= CbKeyUp;
            }
        }

        private static void CbKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var comboBox = (ComboBox) sender;
                comboBox.SelectedItem = null;
            }
        }
    }
}
