using PHmiClient.Utils;
using PHmiModel;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditAlarmCategory.xaml
    /// </summary>
    public partial class EditAlarmCategory : IEditDialog<alarm_categories.AlarmCategoriesMetadata>
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

        public alarm_categories.AlarmCategoriesMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }
    }
}
