using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using PHmiClient.Utils;
using PHmiClient.Utils.Configuration;
using PHmiClient.Utils.ViewInterfaces;
using PHmiRunner.Images;
using PHmiTools;
using PHmiTools.Utils.Npg;

namespace PHmiRunner
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
            ViewModel.View = this;
            ViewModel.PropertyChanged += ViewModelPropertyChanged;
            ViewModel.Reporter.PropertyChanged += ReporterPropertyChanged;
            Loaded += MainWindowLoaded;
            Closed += MainWindowClosed;
            StateChanged += MainWindowStateChanged;
            WindowPositionSettings.UseWith(this, "MainWindowPosition");
            LoadWindowSettings();
        }

        public MainWindowViewModel ViewModel
        {
            get { return (MainWindowViewModel)Resources["ViewModel"]; }
        }

        private void LoadWindowSettings()
        {
            var settings = new Settings("MainWindowSettings");
            miHideAtStart.IsChecked = settings.GetBoolean("HideAtStart") == true;
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            dgNotifications.Items.SortDescriptions.Add(new SortDescription("StartTime", ListSortDirection.Descending));
            _showStateForNotifyIcon = WindowState;
            UpdateConnectionSring();
            if (miHideAtStart.IsChecked)
            {
                WindowState = WindowState.Minimized;
            }
        }

        private void UpdateConnectionSring()
        {
            UpdateConnectionStringFromArgs();
            if (ViewModel.ConnectionParameters == null)
            {
                UpdateConnectionSringFromConfiguration();
            }
        }

        private void UpdateConnectionStringFromArgs()
        {
            var args = ((App) Application.Current).Args;
            if (!string.IsNullOrWhiteSpace(args))
            {
                var connectionParameters = new NpgConnectionParameters();
                connectionParameters.Update(args);
                ViewModel.ConnectionParameters = connectionParameters;
            }
        }
        
        private void UpdateConnectionSringFromConfiguration()
        {
            var connectionStringHelper = new ConnectionStringHelper();
            var configString = connectionStringHelper.Get(PHmiConstants.PHmiConnectionStringName);
            if (configString != null)
            {
                var connectionParameters = new NpgConnectionParameters();
                connectionParameters.Update(configString);
                ViewModel.ConnectionParameters = connectionParameters;
            }
        }

        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ConnectionParameters")
            {
                UpdateIconWithDispatcher();
            }
        }

        private void ReporterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ContainsActiveNotifications")
            {
                UpdateIconWithDispatcher();
            }
        }

        private void UpdateIconWithDispatcher()
        {
            Dispatcher.Invoke(DispatcherPriority.DataBind, new Action(UpdateIcon));
        }

        private void UpdateIcon()
        {
            if (ViewModel.ConnectionParameters == null)
            {
                Icon = IconHelper.GetIcon(ImagesUries.RunitGray);
                if (_notifyIconWrapper != null)
                {
                    _notifyIconWrapper.SetGrayIcon();
                }
            }
            else
            {
                if (ViewModel.Reporter.ContainsActiveNotifications)
                {
                    Icon = IconHelper.GetIcon(ImagesUries.RunitRed);
                    if (_notifyIconWrapper != null)
                    {
                        _notifyIconWrapper.SetRedIcon();
                    }
                }
                else
                {
                    Icon = IconHelper.GetIcon(ImagesUries.Runit);
                    if (_notifyIconWrapper != null)
                    {
                        _notifyIconWrapper.SetGreenIcon();
                    }
                }
            }
        }

        #region NotifyIconWrapper

        private void MainWindowClosed(object sender, EventArgs e)
        {
            if (_notifyIconWrapper != null)
            {
                _notifyIconWrapper.Dispose();
                _notifyIconWrapper = null;
            }
            SaveWindowSettings();
        }

        private void SaveWindowSettings()
        {
            var settings = new Settings("MainWindowSettings");
            settings.SetBoolean("HideAtStart", miHideAtStart.IsChecked);
            settings.Save();
        }

        private void MainWindowStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                _notifyIconWrapper = new NotifyIconWrapper(this, _showStateForNotifyIcon);
                UpdateIcon();
                ShowInTaskbar = false;
            }
            else
            {
                if (_notifyIconWrapper != null)
                {
                    ShowInTaskbar = true;
                    _notifyIconWrapper.Dispose();
                    _notifyIconWrapper = null;
                }
                else
                {
                    _showStateForNotifyIcon = WindowState;
                }
            }
        }

        private WindowState _showStateForNotifyIcon;

        private NotifyIconWrapper _notifyIconWrapper;

        #endregion
    }
}
