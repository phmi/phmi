using System.Collections.Generic;
using PHmiClient.Utils;
using PHmiModel;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditTrendTag.xaml
    /// </summary>
    public partial class EditTrendTag : IEditDialog<trend_tags.TrendTagsMetadata>
    {
        public EditTrendTag()
        {
            this.UpdateLanguage();
            InitializeComponent();
            ViewModel.View = this;
            tbName.Focus();
        }

        public EditTrendTagViewModel ViewModel
        {
            get { return (EditTrendTagViewModel)Resources["ViewModel"]; }
        }

        public trend_tags.TrendTagsMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }

        public IEnumerable<dig_tags> DigitalTags
        {
            get { return ViewModel.DigitalTags; }
            set { ViewModel.DigitalTags = value; }
        }

        public IEnumerable<num_tags> NumericTags
        {
            get { return ViewModel.NumericTags; }
            set { ViewModel.NumericTags = value; }
        } 
    }
}
