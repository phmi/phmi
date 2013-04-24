using System.Windows;
using System.Windows.Input;

namespace PHmiTools.Dialogs.Project
{
    /// <summary>
    /// Interaction logic for OpenProjectDialog.xaml
    /// </summary>
    public partial class OpenProjectDialog
    {
        public OpenProjectDialog()
        {
            InitializeComponent();
            Loaded += OpenProjectDialogLoaded;
        }

        public OpenProjectDialogViewModel ViewModel
        {
            get { return (OpenProjectDialogViewModel)Resources["ViewModel"]; }
        }

        private void OpenProjectDialogLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OpenProjectDialogLoaded;
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

        private void ListBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.OkCommand.Execute(null);
        }
    }
}
