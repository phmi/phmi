using PHmiClient.Utils;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditDigitalTag.xaml
    /// </summary>
    public partial class EditDigitalTag : IEditDialog<DigTag.DigTagMetadata>
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

        public DigTag.DigTagMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }
    }
}
