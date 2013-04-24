using System;
using PHmiClient.Utils;
using PHmiClient.Utils.ViewInterfaces;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for CodeBuilder.xaml
    /// </summary>
    public partial class BuildClient : IWindow
    {
        public BuildClient()
        {
            this.UpdateLanguage();
            InitializeComponent();
            ViewModel.View = this;
            ViewModel.NameSpaceUpdated += ViewModelOnNameSpaceUpdated;
            tbFolder.Focus();
        }

        private void ViewModelOnNameSpaceUpdated(object sender, EventArgs eventArgs)
        {
            tbNameSpace.Focus();
        }

        public BuildClientViewModel ViewModel
        {
            get { return (BuildClientViewModel)Resources["ViewModel"]; }
        }

        public string ConnectionString
        {
            get { return ViewModel.ConnectionString; }
            set { ViewModel.ConnectionString = value; }
        }
    }
}
