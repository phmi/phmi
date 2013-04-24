using System.Collections.Generic;
using PHmiClient.Utils;
using PHmiModel;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditNumericTag.xaml
    /// </summary>
    public partial class EditNumericTag : IEditDialog<num_tags.NumTagsMetadata>
    {
        public EditNumericTag()
        {
            this.UpdateLanguage();
            InitializeComponent();
            ViewModel.View = this;
            tbName.Focus();
        }

        public EditNumericTagViewModel ViewModel
        {
            get { return (EditNumericTagViewModel)Resources["ViewModel"]; }
        }

        public num_tags.NumTagsMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }

        public IEnumerable<num_tag_types> NumTagTypes
        {
            get { return ViewModel.NumTagTypes; }
            set { ViewModel.NumTagTypes = value; }
        }
    }
}
