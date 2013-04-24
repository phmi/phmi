using System.Linq;
using System.Windows.Controls;
using PHmiClient.Utils;
using PHmiClient.Utils.Configuration;
using PHmiClient.Utils.ViewInterfaces;
using PHmiConfigurator.Modules;

namespace PHmiConfigurator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : IWindow
    {
        public MainWindow()
        {
            this.UpdateLanguage();
            InitializeComponent();
            WindowPositionSettings.UseWith(this, "MainWindowPosition");
            ViewModel.View = this;
        }

        public MainWindowViewModel ViewModel
        {
            get { return (MainWindowViewModel) Resources["ViewModel"]; }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var closeCommand = ViewModel.CloseCommand;
            if (!closeCommand.CanExecute(null))
            {
                return;
            }
            closeCommand.Execute(null);
            if (ViewModel.Modules.Any())
            {
                e.Cancel = true;
            }
        }

        private void TabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = (TabControl) sender;
            var moduleBase = tabControl.SelectedItem as Module;
            ViewModel.SelectedModuleViewModel = moduleBase == null ? null : moduleBase.ViewModel;
        }
    }
}
