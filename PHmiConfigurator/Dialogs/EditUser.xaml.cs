using PHmiClient.Utils;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditUser.xaml
    /// </summary>
    public partial class EditUser : IEditDialog<User.UserMetadata>
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

        public User.UserMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }
    }
}