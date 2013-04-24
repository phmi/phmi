using System.Windows;
using PHmiClient.Utils;
using PHmiClient.Utils.ViewInterfaces;
using PHmiTools.Utils.Npg;

namespace PHmiTools.Dialogs.Project
{
    /// <summary>
    /// Interaction logic for ExportProjectDialog.xaml
    /// </summary>
    public partial class ExportProjectDialog : IWindow
    {
        public ExportProjectDialog(INpgConnectionParameters connectionParameters)
        {
            this.UpdateLanguage();
            Owner = Application.Current.MainWindow;
            InitializeComponent();
            ViewModel.ConnectionParameters = connectionParameters;
            ViewModel.View = this;
            Loaded += ExportProjectDialogLoaded;
        }

        private void ExportProjectDialogLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ExportProjectDialogLoaded;
            ViewModel.Export();
        }

        public ExportProjectDialogViewModel ViewModel
        {
            get { return (ExportProjectDialogViewModel) Resources["ViewModel"]; }
        }
    }
}
