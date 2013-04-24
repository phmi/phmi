using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using PHmiIoDeviceTools;
using Path = System.IO.Path;

namespace PHmiTools.Controls
{
    /// <summary>
    /// Interaction logic for IoDeviceItem.xaml
    /// </summary>
    public partial class IoDeviceItem
    {
        private readonly string _path;

        public IoDeviceItem(string path)
        {
            InitializeComponent();
            _path = path;
            var fileName = Path.GetFileName(_path);
            ItemName = fileName == null
                ? string.Empty
                : Path.GetFileNameWithoutExtension(fileName.Substring(PHmiConstants.PHmiIoDevicePrefix.Length));
            tb.Text = ItemName;
            Loaded += IoDeviceItemLoaded;
        }

        private void IoDeviceItemLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= IoDeviceItemLoaded;
            var parent = Parent as IoDeviceDirectoryItem;
            if (parent == null)
            {
                LoadIoDeviceImage();
            }
            else
            {
                parent.Expanded += ParentExpanded;
            }
        }

        private void ParentExpanded(object sender, RoutedEventArgs e)
        {
            var parent = (IoDeviceDirectoryItem)sender;
            parent.Expanded -= ParentExpanded;
            LoadIoDeviceImage();
        }

        private void LoadIoDeviceImage()
        {
            Action action = LoadImage;
            action.BeginInvoke(action.EndInvoke, null);
        }

        private void LoadImage()
        {
            var assembly = GetAssembly(_path);
            if (assembly == null)
                return;
            Dispatcher.Invoke(new Action(() =>
                {
                    var bitmapImage = GetImage(assembly);
                    if (bitmapImage != null)
                    {
                        image.Source = bitmapImage;
                    }
                }));
        }

        public string ItemName { get; private set; }

        public string Type
        {
            get
            {
                var path = GetPath(new List<string>(), Parent as IoDeviceDirectoryItem);
                path.Add(ItemName);
                return string.Join(PHmiConstants.PHmiIoDeviceSeparator.ToString(CultureInfo.InvariantCulture), path);
            }
        }

        private static List<string> GetPath(List<string> path, IoDeviceDirectoryItem item)
        {
            if (item == null)
                return path;
            path.Insert(0, item.ItemName);
            var parent = item.Parent as IoDeviceDirectoryItem;
            return parent != null ? GetPath(path, parent) : path;
        }

        private static Assembly GetAssembly(string file)
        {
            try
            {
                return Assembly.LoadFrom(file);
            }
            catch
            {
                return null;
            }
        }

        private static BitmapImage GetImage(Assembly assembly)
        {
            var resourceNames = assembly.GetManifestResourceNames();
            var streams = (from r in resourceNames
                           where r.EndsWith(PHmiConstants.PHmiIoDeviceImageName)
                           select assembly.GetManifestResourceStream(r)
                           into stream where stream != null select stream);
            foreach (var stream in streams)
            {
                var image = new BitmapImage();
                try
                {
                    image.BeginInit();
                    image.StreamSource = stream;
                    image.EndInit();
                }
                catch
                {
                    continue;
                }
                return image;
            }
            return null;
        }

        private static IOptionsEditor GetEditor(Assembly assembly)
        {
            try
            {
                var types = (from t in assembly.GetTypes()
                             where typeof(IOptionsEditor).IsAssignableFrom(t) && !t.IsAbstract && t.IsPublic
                             select t).ToArray();
                return (from type in types
                        select type.GetConstructor(System.Type.EmptyTypes) into ctor 
                        where ctor != null
                        select (IOptionsEditor) ctor.Invoke(null)).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public IOptionsEditor GetEditor()
        {
            var assembly = GetAssembly(_path);
            return assembly == null ? null : GetEditor(assembly);
        }
    }
}
