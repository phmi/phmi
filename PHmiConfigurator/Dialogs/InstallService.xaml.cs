using PHmiClient.Utils;
using PHmiClient.Utils.ViewInterfaces;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for InstallService.xaml
    /// </summary>
    public partial class InstallService : IWindow
    {
        public InstallService()
        {
            this.UpdateLanguage();
            InitializeComponent();
            ViewModel.View = this;
            Loaded += InstallServiceLoaded;
        }

        private void InstallServiceLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (rbUseProjectConnectionString.IsChecked == true)
            {
                bInstall.Focus();
            }
            else
            {
                tbCustomConnectionString.Focus();
            }
        }

        public InstallServiceViewModel ViewModel
        {
            get { return (InstallServiceViewModel) Resources["ViewModel"]; }
        }

        public string ProjectConnectionString
        {
            get { return ViewModel.ProjectConnectionString; }
            set { ViewModel.ProjectConnectionString = value; }
        }
    }
}
