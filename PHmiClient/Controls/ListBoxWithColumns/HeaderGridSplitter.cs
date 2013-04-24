using System.Windows;
using System.Windows.Controls;

namespace PHmiClient.Controls.ListBoxWithColumns
{
    public class HeaderGridSplitter: GridSplitter
    {
        public HeaderGridSplitter()
        {
            MouseDoubleClick += HeaderGridSplitterMouseDoubleClick;
        }

        private void HeaderGridSplitterMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ColumnDefinition != null)
                ColumnDefinition.Width = new GridLength();
        }

        public ColumnDefinition ColumnDefinition
        {
            get { return (ColumnDefinition)GetValue(ColumnDefinitionProperty); }
            set { SetValue(ColumnDefinitionProperty, value); }
        }

        public static readonly DependencyProperty ColumnDefinitionProperty =
            DependencyProperty.Register("ColumnDefinition", typeof(ColumnDefinition), typeof(HeaderGridSplitter), null);
    }
}
