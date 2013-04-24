using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace PHmiClient.Controls
{
    public static class DataGridBehavior
    {
        #region RowNumbers property

        public static readonly DependencyProperty RowNumbersProperty =
            DependencyProperty.RegisterAttached("RowNumbers", typeof(bool), typeof(DataGridBehavior),
            new FrameworkPropertyMetadata(false, OnRowNumbersChanged));

        private static void OnRowNumbersChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            var grid = source as DataGrid;
            if (grid == null)
                return;
            if ((bool)args.NewValue)
            {
                grid.LoadingRow += OnGridLoadingRow;
                grid.UnloadingRow += OnGridUnloadingRow;
            }
            else
            {
                grid.LoadingRow -= OnGridLoadingRow;
                grid.UnloadingRow -= OnGridUnloadingRow;
            }
        }

        private static void RefreshDataGridRowNumbers(object sender)
        {
            var grid = sender as DataGrid;
            if (grid == null)
                return;

            foreach (var item in grid.Items)
            {
                var row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(item);
                if (row != null)
                    row.Header = row.GetIndex() + 1;
            }
        }

        private static void OnGridUnloadingRow(object sender, DataGridRowEventArgs e)
        {
            RefreshDataGridRowNumbers(sender);
        }

        private static void OnGridLoadingRow(object sender, DataGridRowEventArgs e)
        {
            RefreshDataGridRowNumbers(sender);
        }

        public static void SetRowNumbers(DependencyObject element, bool value)
        {
            element.SetValue(RowNumbersProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static bool GetRowNumbers(DependencyObject element)
        {
            return (bool)element.GetValue(RowNumbersProperty);
        }

        #endregion

        #region SupportIDataErrorInfo

        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static bool GetSupportIDataErrorInfo(DependencyObject obj)
        {
            return (bool)obj.GetValue(SupportIDataErrorInfoProperty);
        }

        public static void SetSupportIDataErrorInfo(DependencyObject obj, bool value)
        {
            obj.SetValue(SupportIDataErrorInfoProperty, value);
        }

        public static readonly DependencyProperty SupportIDataErrorInfoProperty =
            DependencyProperty.RegisterAttached("SupportIDataErrorInfo", typeof(bool), typeof(DataGridBehavior),
            new FrameworkPropertyMetadata(false, OnSupportIDataErrorInfoChanged));

        private static void OnSupportIDataErrorInfoChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            var grid = source as DataGrid;
            if (grid == null)
                return;
            if ((bool)args.NewValue)
            {
                grid.RowValidationRules.Add(new DataErrorInfoValidationRule {ValidationStep = ValidationStep.UpdatedValue});
            }
            else
            {
                var rules = grid.RowValidationRules.Where(r => r.GetType() == typeof (DataErrorInfoValidationRule)).ToArray();
                foreach (var r in rules)
                {
                    grid.RowValidationRules.Remove(r);
                }
            }
        }

        #endregion

        #region SelectedItems

        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
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
            DependencyProperty.RegisterAttached("SelectedItems", typeof (IList), typeof (DataGridBehavior),
            new FrameworkPropertyMetadata(OnSelectedItemsPropertyChanged));

        private static void OnSelectedItemsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            var grid = source as DataGrid;
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
            var grid = (DataGrid) sender;
            UpdateSelectedItems(grid);
        }

        private static void UpdateSelectedItems(MultiSelector grid)
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

        #region ScrollToSelected

        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static bool GetScrollToSelected(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollToSelectedProperty);
        }

        public static void SetScrollToSelected(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollToSelectedProperty, value);
        }

        public static readonly DependencyProperty ScrollToSelectedProperty =
            DependencyProperty.RegisterAttached("ScrollToSelected", typeof(bool), typeof(DataGridBehavior),
            new FrameworkPropertyMetadata(false, OnScrollToSelectedChanged));

        private static void OnScrollToSelectedChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            var grid = source as DataGrid;
            if (grid == null)
                return;
            if ((bool)args.NewValue)
            {
                grid.SelectionChanged += GridScrollToSelectedSelectionChanged;
            }
            else
            {
                grid.SelectionChanged -= GridScrollToSelectedSelectionChanged;
            }
        }

        private static void GridScrollToSelectedSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var grid = (DataGrid) sender;
            if (grid.SelectedItem != null)
                grid.ScrollIntoView(grid.SelectedItem);
        }
        
        #endregion

        #region ThreeStateSrorting

        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static bool GetThreeStateSorting(DependencyObject obj)
        {
            return (bool)obj.GetValue(ThreeStateSortingProperty);
        }

        public static void SetThreeStateSorting(DependencyObject obj, bool value)
        {
            obj.SetValue(ThreeStateSortingProperty, value);
        }

        public static readonly DependencyProperty ThreeStateSortingProperty =
            DependencyProperty.RegisterAttached("ThreeStateSorting", typeof(bool), typeof(DataGridBehavior),
            new FrameworkPropertyMetadata(false, OnThreeStateSortingChanged));
        
        private static void OnThreeStateSortingChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            var grid = source as DataGrid;
            if (grid == null)
                return;
            if ((bool)args.NewValue)
            {
                grid.Sorting += GridSorting;
            }
            else
            {
                grid.Sorting -= GridSorting;
            }
        }

        private static void GridSorting(object sender, DataGridSortingEventArgs e)
        {
            var dataGrid = (DataGrid) sender;

            var sortPropertyName = GetSortMemberPath(e.Column);
            if (!string.IsNullOrEmpty(sortPropertyName))
            {
                // sorting is cleared when the previous state is Descending
                if (e.Column.SortDirection.HasValue && e.Column.SortDirection.Value == ListSortDirection.Descending)
                {
                    var index = FindSortDescription(dataGrid.Items.SortDescriptions, sortPropertyName);
                    if (index != -1)
                    {
                        e.Column.SortDirection = null;

                        // remove the sort description
                        dataGrid.Items.SortDescriptions.RemoveAt(index);
                        dataGrid.Items.Refresh();

                        if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
                        {
                            // clear any other sort descriptions for the multisorting case
                            dataGrid.Items.SortDescriptions.Clear();
                            dataGrid.Items.Refresh();
                        }

                        // stop the default sort
                        e.Handled = true;
                    }
                }
            }
        }

        private static string GetSortMemberPath(DataGridColumn column)
        {
            // find the sortmemberpath
            var sortPropertyName = column.SortMemberPath;
            if (string.IsNullOrEmpty(sortPropertyName))
            {
                var boundColumn = column as DataGridBoundColumn;
                if (boundColumn != null)
                {
                    var binding = boundColumn.Binding as Binding;
                    if (binding != null)
                    {
                        if (!string.IsNullOrEmpty(binding.XPath))
                        {
                            sortPropertyName = binding.XPath;
                        }
                        else if (binding.Path != null)
                        {
                            sortPropertyName = binding.Path.Path;
                        }
                    }
                }
            }

            return sortPropertyName;
        }

        private static int FindSortDescription(IEnumerable<SortDescription> sortDescriptions, string sortPropertyName)
        {
            var index = -1;
            var i = 0;
            foreach (var sortDesc in sortDescriptions)
            {
                if (System.String.CompareOrdinal(sortDesc.PropertyName, sortPropertyName) == 0)
                {
                    index = i;
                    break;
                }
                i++;
            }
            return index;
        }
        
        #endregion

        #region ControlCCommand

        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static ICommand GetControlCCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(ControlCCommandProperty);
        }

        public static void SetControlCCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(ControlCCommandProperty, value);
        }

        public static readonly DependencyProperty ControlCCommandProperty =
            DependencyProperty.RegisterAttached("ControlCCommand", typeof(ICommand), typeof(DataGridBehavior),
            new FrameworkPropertyMetadata(null, OnControlCCommandChanged));

        private static void OnControlCCommandChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            var grid = source as DataGrid;
            if (grid == null)
                return;
            if (args.OldValue == null && args.NewValue != null)
                grid.PreviewKeyDown += GridControlCCommandPreviewKeyDown;
            if (args.OldValue != null && args.NewValue == null)
                grid.PreviewKeyDown -= GridControlCCommandPreviewKeyDown;
        }

        private static void GridControlCCommandPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource != null
                && e.OriginalSource.GetType() == typeof(DataGridCell)
                && e.Key == Key.C
                && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var grid = (DataGrid) sender;
                var command = grid.GetValue(ControlCCommandProperty) as ICommand;
                if (command != null && command.CanExecute(null))
                {
                    command.Execute(null);
                    e.Handled = true;
                }
            }
        }

        #endregion

        #region ControVCommand

        [AttachedPropertyBrowsableForType(typeof(DataGrid))]
        public static ICommand GetControlVCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(ControlVCommandProperty);
        }

        public static void SetControlVCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(ControlVCommandProperty, value);
        }

        public static readonly DependencyProperty ControlVCommandProperty =
            DependencyProperty.RegisterAttached("ControlVCommand", typeof(ICommand), typeof(DataGridBehavior),
            new FrameworkPropertyMetadata(null, OnControlVCommandChanged));

        private static void OnControlVCommandChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
        {
            var grid = source as DataGrid;
            if (grid == null)
                return;
            if (args.OldValue == null && args.NewValue != null)
                grid.PreviewKeyDown += GridControlVCommandPreviewKeyDown;
            if (args.OldValue != null && args.NewValue == null)
                grid.PreviewKeyDown -= GridControlVCommandPreviewKeyDown;
        }

        private static void GridControlVCommandPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource != null
                && e.OriginalSource.GetType() == typeof(DataGridCell)
                && e.Key == Key.V
                && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                var grid = (DataGrid)sender;
                var command = grid.GetValue(ControlVCommandProperty) as ICommand;
                if (command != null && command.CanExecute(null))
                {
                    command.Execute(null);
                    e.Handled = true;
                }
            }
        }

        #endregion
    }
}
