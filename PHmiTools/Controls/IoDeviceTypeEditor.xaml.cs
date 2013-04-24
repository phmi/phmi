using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using PHmiClient.Utils;
using PHmiIoDeviceTools;

namespace PHmiTools.Controls
{
    /// <summary>
    /// Interaction logic for IoDeviceTypeEditor.xaml
    /// </summary>
    public partial class IoDeviceTypeEditor : UserControl
    {
        public IoDeviceTypeEditor()
        {
            InitializeComponent();
            Loaded += IoDeviceTypeEditorLoaded;
        }

        private void IoDeviceTypeEditorLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= IoDeviceTypeEditorLoaded;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var path = string.Join(
                    Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture),
                    Path.GetDirectoryName(Assembly.GetAssembly(typeof (IoDeviceTypeEditor)).Location),
                    PHmiConstants.PHmiIoDevicesDirectoryName);
                foreach (var i in IoDeviceDirectoryItem.GetItems(path))
                {
                    treeView.Items.Add(i);
                }
                Action action = () =>
                    UpdateType(Type);
                Dispatcher.BeginInvoke(action);
            }
        }

        #region Type
        
        public string Type
        {
            get { return (string)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(string), typeof(IoDeviceTypeEditor),
            new PropertyMetadata(OnTypeChangedCallback));

        private static void OnTypeChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var editor = (IoDeviceTypeEditor) obj;
            editor.UpdateType(args.NewValue as string);
        }

        #endregion

        private void UpdateType(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                foreach (TreeViewItem item in treeView.Items)
                {
                    Unselect(item);
                }
                return;
            }
            var path = type.Split(PHmiConstants.PHmiIoDeviceSeparator);
            var stackPath = new Stack<string>(path.Length);
            foreach (var s in path.Reverse())
            {
                stackPath.Push(s);
            }
            Select(stackPath, treeView.Items.Cast<TreeViewItem>().ToArray());
        }

        private static void Unselect(TreeViewItem item)
        {
            item.IsSelected = false;
            foreach (TreeViewItem i in item.Items)
            {
                Unselect(i);
            }
        }

        private void Select(Stack<string> path, TreeViewItem[] items)
        {
            var s = path.Pop();
            if (path.Any())
            {
                foreach (var item in items.OfType<IoDeviceDirectoryItem>().Where(item => item.ItemName == s))
                {
                    item.IsExpanded = true;
                    Action action = () =>
                        Select(path, item.Items.Cast<TreeViewItem>().ToArray());
                    Dispatcher.BeginInvoke(action);
                    break;
                }
            }
            else
            {
                foreach (var item in items.OfType<IoDeviceItem>().Where(item => item.ItemName == s))
                {
                    item.IsSelected = true;
                    break;
                }
            }
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = e.NewValue as IoDeviceItem;
            if (item != null)
                Type = item.Type;
            EventHelper.Raise(ref SelectedItemChanged, this, EventArgs.Empty);
        }

        public event EventHandler SelectedItemChanged;

        public IOptionsEditor CreateOptionsEditor()
        {
            var item = treeView.SelectedItem as IoDeviceItem;
            return item == null ? null : item.GetEditor();
        }
    }
}
