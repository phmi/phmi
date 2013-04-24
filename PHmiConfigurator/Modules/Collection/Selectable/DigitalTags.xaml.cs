using System.Windows.Controls;

namespace PHmiConfigurator.Modules.Collection.Selectable
{
    /// <summary>
    /// Interaction logic for DigitalTags.xaml
    /// </summary>
    public partial class DigitalTags 
    {
        public DigitalTags()
        {
            InitializeComponent();
        }

        private void CbSelectorsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbSelectors.SelectionChanged -= CbSelectorsSelectionChanged;
            if (cbSelectors.SelectedItem != null)
                cbSelectors.IsDropDownOpen = false;
        }
    }
}
