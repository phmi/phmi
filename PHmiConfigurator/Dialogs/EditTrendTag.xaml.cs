using System.Collections.Generic;
using PHmiClient.Utils;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditTrendTag.xaml
    /// </summary>
    public partial class EditTrendTag : IEditDialog<TrendTag.TrendTagMetadata>
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

        public TrendTag.TrendTagMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }

        public IEnumerable<DigTag> DigitalTags
        {
            get { return ViewModel.DigitalTags; }
            set { ViewModel.DigitalTags = value; }
        }

        public IEnumerable<NumTag> NumericTags
        {
            get { return ViewModel.NumericTags; }
            set { ViewModel.NumericTags = value; }
        } 
    }
}
