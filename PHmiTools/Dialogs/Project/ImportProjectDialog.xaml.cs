using System.Windows;

namespace PHmiTools.Dialogs.Project
{
    /// <summary>
    /// Interaction logic for ImportProjectDialog.xaml
    /// </summary>
    public partial class ImportProjectDialog
    {
        public ImportProjectDialog()
        {
            InitializeComponent();
            Loaded += ImportProjectDialogLoaded;
        }

        public ImportProjectDialogViewModel ViewModel
        {
            get { return (ImportProjectDialogViewModel)Resources["ViewModel"]; }
        }

        private void ImportProjectDialogLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowChooseDialog(true);
            Loaded -= ImportProjectDialogLoaded;
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
