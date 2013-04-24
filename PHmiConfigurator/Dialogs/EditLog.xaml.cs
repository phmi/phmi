using PHmiClient.Utils;
using PHmiModel;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditLog.xaml
    /// </summary>
    public partial class EditLog : IEditDialog<logs.LogsMetadata>
    {
        public EditLog()
        {
            this.UpdateLanguage();
            InitializeComponent();
            ViewModel.View = this;
            tbName.Focus();
        }

        public EditLogViewModel ViewModel
        {
            get { return (EditLogViewModel)Resources["ViewModel"]; }
        }

        public logs.LogsMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }
    }
}
