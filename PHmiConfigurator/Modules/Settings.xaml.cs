
namespace PHmiConfigurator.Modules
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();
            Loaded += SettingsLoaded;
        }

        private void SettingsLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Loaded -= SettingsLoaded;
            TbServer.Focus();
        }
    }
}
