using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;

namespace PHmiTools.Controls
{
    /// <summary>
    /// Interaction logic for IoDeviceDirectoryItem.xaml
    /// </summary>
    public partial class IoDeviceDirectoryItem
    {
        private readonly string _path;

        public IoDeviceDirectoryItem(string path)
        {
            InitializeComponent();
            _path = path;
            ShowClosed();
            Expanded += IoDeviceDirectoryItemExpanded;
            Collapsed += IoDeviceDirectoryItemCollapsed;
            var fileName = Path.GetFileName(_path);
            ItemName = fileName == null
                ? string.Empty
                : fileName.Substring(PHmiConstants.PHmiIoDevicePrefix.Length);
            foreach (var item in GetItems(_path))
            {
                Items.Add(item);
            }
            tb.Text = ItemName;
        }

        private void IoDeviceDirectoryItemCollapsed(object sender, RoutedEventArgs e)
        {
            if (ReferenceEquals(e.OriginalSource, this))
            {
                ShowClosed();
            }
        }

        private void ShowClosed()
        {
            image.Source = new BitmapImage(PHmiResources.ImagesUries.FolderClosedPng);
        }

        private void IoDeviceDirectoryItemExpanded(object sender, RoutedEventArgs e)
        {
            if (ReferenceEquals(e.OriginalSource, this))
            {
                ShowOpen();
            }
        }

        private void ShowOpen()
        {
            image.Source = new BitmapImage(PHmiResources.ImagesUries.FolderOpenPng);
        }

        public static TreeViewItem[] GetItems(string path)
        {
            var result = new List<TreeViewItem>();
            result.AddRange(GetValidPaths(Directory.GetDirectories(path)).Select(p => new IoDeviceDirectoryItem(p)));
            result.AddRange(GetValidPaths(
                Directory.GetFiles(path)).Where(p => Path.GetExtension(p) == PHmiConstants.PHmiIoDeviceExt)
                .Select(p => new IoDeviceItem(p)));
            return result.ToArray();
        }

        private static IEnumerable<string> GetValidPaths(IEnumerable<string> paths)
        {
            return paths.Where(IsValidPath).OrderBy(p => p).ToArray();
        }

        private static bool IsValidPath(string path)
        {
            var fileName = Path.GetFileName(path);
            return fileName != null && fileName.StartsWith(PHmiConstants.PHmiIoDevicePrefix);
        }

        public string ItemName { get; private set; }
    }
}
