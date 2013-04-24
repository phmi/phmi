using System.Windows;

namespace PHmiTools.Dialogs.Project
{
    /// <summary>
    /// Interaction logic for NewProjectDialog.xaml
    /// </summary>
    public partial class NewProjectDialog
    {
        public NewProjectDialog()
        {
            InitializeComponent();
            Loaded += NewProjectDialogLoaded;
        }

        public NewProjectDialogViewModel ViewModel
        {
            get { return (NewProjectDialogViewModel)Resources["ViewModel"]; }
        }

        private void NewProjectDialogLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= NewProjectDialogLoaded;
            pbPasswrod.Password = ViewModel.ConnectionParameters.Password;
            pbPasswrod.PasswordChanged += PbPasswrodPasswordChanged;
            if (string.IsNullOrEmpty(tbServer.Text))
                tbServer.Focus();
            else if (string.IsNullOrEmpty(tbPort.Text))
                tbPort.Focus();
            else if (string.IsNullOrEmpty(tbUserId.Text))
                tbUserId.Focus();
            else if (string.IsNullOrEmpty(pbPasswrod.Password))
                pbPasswrod.Focus();
            else
                tbDatabase.Focus();
        }

        private void PbPasswrodPasswordChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.ConnectionParameters.Password = pbPasswrod.Password;
        }
    }
}
