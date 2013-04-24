using System.Windows.Controls;

namespace PHmiConfigurator.Modules.Collection.Selectable
{
    /// <summary>
    /// Interaction logic for AlarmTags.xaml
    /// </summary>
    public partial class AlarmTags
    {
        public AlarmTags()
        {
            InitializeComponent();
        }

        private void ComboBoxLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var comboBox = (ComboBox) sender;
            comboBox.Loaded -= ComboBoxLoaded;
            comboBox.IsDropDownOpen = true;
        }

        private void CbSelectorsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbSelectors.SelectionChanged -= CbSelectorsSelectionChanged;
            if (cbSelectors.SelectedItem != null)
                cbSelectors.IsDropDownOpen = false;
        }
    }
}
