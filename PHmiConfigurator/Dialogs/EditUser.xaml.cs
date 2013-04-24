using PHmiClient.Utils;
using PHmiModel;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditUser.xaml
    /// </summary>
    public partial class EditUser : IEditDialog<users.UsersMetadata>
    {
        public EditUser()
        {
            this.UpdateLanguage();
            InitializeComponent();
            ViewModel.View = this;
            TbName.Focus();
        }

        public EditUserViewModel ViewModel
        {
            get { return (EditUserViewModel) Resources["ViewModel"]; }
        }

        public users.UsersMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }
    }
}