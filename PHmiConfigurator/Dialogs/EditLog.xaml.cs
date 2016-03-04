using PHmiClient.Utils;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditLog.xaml
    /// </summary>
    public partial class EditLog : IEditDialog<Log.LogMetadata>
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

        public Log.LogMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }
    }
}
