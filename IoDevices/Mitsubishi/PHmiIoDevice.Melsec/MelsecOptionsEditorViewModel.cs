using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using PHmiClient.Controls.Input;
using PHmiClient.Utils;
using PHmiIoDevice.Melsec.Configuration;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;

namespace PHmiIoDevice.Melsec
{
    public class MelsecOptionsEditorViewModel : INotifyPropertyChanged
    {
        private Config _config;
        private ConfigType _configType;
        private ICommand _showDocCommand;

        public Config Config
        {
            get { return _config; }
            set
            {
                OnConfigChanging(_config, value);
                _config = value;
                OnPropertyChanged("Config");
                OnPropertyChanged("EnetConfig");
                OnPropertyChanged("FxComConfig");
                OnPropertyChanged("FxEnetConfig");
                OnPropertyChanged("QConfig");
                UpdateConfigType();
                OnConfigChanged();
            }
        }

        private void OnConfigChanging(INotifyPropertyChanged oldValue, INotifyPropertyChanged newValue)
        {
            if (oldValue != null)
            {
                oldValue.PropertyChanged -= OnConfigPropertyChanged;
            }
            newValue.PropertyChanged += OnConfigPropertyChanged;
        }

        private void OnConfigPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnConfigChanged();
        }

        private void OnConfigChanged()
        {
            EventHelper.Raise(ref ConfigChanged, this, EventArgs.Empty);
        }

        private void UpdateConfigType()
        {
            if (FxComConfig != null)
            {
                ConfigType = ConfigType.FxCom;
            }
            else if (FxEnetConfig != null)
            {
                ConfigType = ConfigType.FxEnet;
            }
            else if (QConfig != null)
            {
                ConfigType = ConfigType.Q;
            }
        }

        public EnetConfig EnetConfig { get { return _config as EnetConfig; } }

        public FxComConfig FxComConfig { get { return _config as FxComConfig; } }

        public FxEnetConfig FxEnetConfig { get { return _config as FxEnetConfig; } }

        public QConfig QConfig { get { return _config as QConfig; } }

        public ConfigType ConfigType
        {
            get { return _configType; }
            set
            {
                _configType = value;
                OnPropertyChanged("ConfigType");
                UpdateConfig();
            }
        }

        private void UpdateConfig()
        {
            switch (ConfigType)
            {
                case ConfigType.FxCom:
                    if (FxComConfig == null)
                    {
                        Config = new FxComConfig();
                    }
                    break;
                case ConfigType.FxEnet:
                    if (FxEnetConfig == null)
                    {
                        Config = new FxEnetConfig();
                    }
                    break;
                case ConfigType.Q:
                    if (QConfig == null)
                    {
                        Config = new QConfig();
                    }
                    break;
                default:
                    throw new NotSupportedException(ConfigType.ToString());
            }
        }

        public event EventHandler ConfigChanged;

        public string[] ComPorts
        {
            get { return SerialPort.GetPortNames().OrderBy(p => p).ToArray(); }
        }

        public int[] BaudRates
        {
            get
            {
                return new []
                    {
                        75, 110, 300, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200
                    };
            }
        }

        public int[] DataBits
        {
            get
            {
                return new []
                    {
                        7, 8
                    };
            }
        }

        public ICommand ShowDocCommand
        {
            get
            {
                return _showDocCommand ?? (_showDocCommand = new DelegateCommand(ShowDocCommandExecuted));
            }
        }

        private static void ShowDocCommandExecuted(object obj)
        {
            try
            {
                var assemblyLocation = Assembly.GetAssembly(typeof (MelsecOptionsEditorViewModel)).Location;
                var dirPath = Path.GetDirectoryName(assemblyLocation);
                var fileName = Path.GetFileNameWithoutExtension(assemblyLocation) + ".pdf";
                if (dirPath == null)
                    return;
                var filePath = Path.Combine(dirPath, fileName);
                Process.Start(filePath);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
