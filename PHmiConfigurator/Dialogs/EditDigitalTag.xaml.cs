using PHmiClient.Utils;
using PHmiModel;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditDigitalTag.xaml
    /// </summary>
    public partial class EditDigitalTag : IEditDialog<dig_tags.DigTagsMetadata>
    {
        public EditDigitalTag()
        {
            this.UpdateLanguage();
            InitializeComponent();
            ViewModel.View = this;
            tbName.Focus();
        }

        public EditDigitalTagViewModel ViewModel
        {
            get { return (EditDigitalTagViewModel)Resources["ViewModel"]; }
        }

        public dig_tags.DigTagsMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }
    }
}
