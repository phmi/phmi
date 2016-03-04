using PHmiClient.Utils;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditAlarmCategory.xaml
    /// </summary>
    public partial class EditAlarmCategory : IEditDialog<AlarmCategory.AlarmCategoryMetadata>
    {
        public EditAlarmCategory()
        {
            this.UpdateLanguage();
            InitializeComponent();
            ViewModel.View = this;
            tbName.Focus();
        }

        public EditAlarmCategoryViewModel ViewModel
        {
            get { return (EditAlarmCategoryViewModel) Resources["ViewModel"]; }
        }

        public AlarmCategory.AlarmCategoryMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }
    }
}
