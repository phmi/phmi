using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace PHmiClient.Controls
{
    public static class ListBoxBehavior
    {
        #region SelectedItems

        [AttachedPropertyBrowsableForType(typeof(ListBox))]
        public static IList GetSelectedItems(DependencyObject obj)
        {
            return (IList)obj.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DependencyObject obj, IList value)
        {
            obj.SetValue(SelectedItemsProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems", typeof(IList), typeof(ListBoxBehavior),
            new FrameworkPropertyMetadata(OnSelectedItemsPropertyChanged));

        private static void OnSelectedItemsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            var grid = source as ListBox;
            if (grid == null)
                return;
            if (args.OldValue != null && args.NewValue == null)
            {
                grid.SelectionChanged -= GridSelectedItemsSelectionChanged;
            }
            if (args.OldValue == null && args.NewValue != null)
            {
                grid.SelectionChanged += GridSelectedItemsSelectionChanged;
                UpdateSelectedItems(grid);
            }
        }

        private static void GridSelectedItemsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = (ListBox)sender;
            UpdateSelectedItems(grid);
        }

        private static void UpdateSelectedItems(ListBox grid)
        {
            var list = GetSelectedItems(grid);
            for (var i = 0; i < grid.SelectedItems.Count; i++)
            {
                var gridItem = grid.SelectedItems[i];
                while (list.Count > i && !AreEqual(gridItem, list[i]))
                {
                    list.RemoveAt(i);
                }
                if (list.Count <= i || !AreEqual(gridItem, list[i]))
                    list.Insert(i, gridItem);
            }
            if (grid.SelectedItems.Count == 0)
                list.Clear();
        }

        private static bool AreEqual(object obj1, object obj2)
        {
            if (obj1 == null)
                return obj2 == null;
            return obj1.Equals(obj2);
        }

        #endregion
    }
}
