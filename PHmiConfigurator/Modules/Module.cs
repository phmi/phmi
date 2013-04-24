using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PHmiClient.Utils;

namespace PHmiConfigurator.Modules
{
    public class Module : TabItem, IModule
    {
        public Module()
        {
            Loaded += ModuleBaseLoaded;
        }

        private void ModuleBaseLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ModuleBaseLoaded;
            ViewModel.View = this;
            ViewModel.Closed += ViewModelClosed;
            ViewModel.ConnectionString = ConnectionString;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Dispatcher.BeginInvoke(new Action(() => ViewModel.Reload()));
            }
        }

        public string ConnectionString { get; set; }

        public event EventHandler Closed;

        public bool HasChanges { get { return ViewModel.HasChanges; } }

        public bool IsValid { get { return ViewModel.IsValid; } }

        public void Save()
        {
            ViewModel.Save();
        }

        public IModuleViewModel ViewModel { get { return (IModuleViewModel) Resources["ViewModel"]; } }

        private void ViewModelClosed(object sender, EventArgs e)
        {
            EventHelper.Raise(ref Closed, this, EventArgs.Empty);
        }

        public ImageSource ImageSource { get; set; }
    }
}
