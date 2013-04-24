using PHmiClient.Controls.Input;
using PHmiClient.Utils;
using PHmiIoDevice.Modbus.Configuration;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace PHmiIoDevice.Modbus
{
    public class ModbusOptionsEditorViewModel : INotifyPropertyChanged
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
                OnPropertyChanged(m => m.Config);
                OnPropertyChanged(m => m.AsciiConfig);
                OnPropertyChanged(m => m.AsciiViaTcpConfig);
                OnPropertyChanged(m => m.ComConfig);
                OnPropertyChanged(m => m.EnetConfig);
                OnPropertyChanged(m => m.RtuConfig);
                OnPropertyChanged(m => m.RtuViaTcpConfig);
                OnPropertyChanged(m => m.TcpConfig);
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
            if (AsciiConfig != null)
            {
                ConfigType = ConfigType.Ascii;
            }
            else if (AsciiViaTcpConfig != null)
            {
                ConfigType = ConfigType.AsciiViaTcp;
            }
            else if (RtuConfig != null)
            {
                ConfigType = ConfigType.Rtu;
            }
            else if (RtuViaTcpConfig != null)
            {
                ConfigType = ConfigType.RtuViaTcp;
            }
            else if (TcpConfig != null)
            {
                ConfigType = ConfigType.Tcp;
            }
        }

        public AsciiConfig AsciiConfig { get { return _config as AsciiConfig; } }

        public AsciiViaTcpConfig AsciiViaTcpConfig { get { return _config as AsciiViaTcpConfig; } }

        public ComConfig ComConfig { get { return _config as ComConfig; } }

        public EnetConfig EnetConfig { get { return _config as EnetConfig; } }

        public RtuConfig RtuConfig { get { return _config as RtuConfig; } }

        public RTUviaTCPConfig RtuViaTcpConfig { get { return _config as RTUviaTCPConfig; } }

        public TcpConfig TcpConfig { get { return _config as TcpConfig; } }

        public ConfigType ConfigType
        {
            get { return _configType; }
            set
            {
                _configType = value;
                OnPropertyChanged(m => m.ConfigType);
                UpdateConfig();
            }
        }

        private void UpdateConfig()
        {
            switch (ConfigType)
            {
                case ConfigType.Ascii:
                    if (AsciiConfig == null)
                    {
                        Config = new AsciiConfig();
                    }
                    break;
                case ConfigType.AsciiViaTcp:
                    if (AsciiViaTcpConfig == null)
                    {
                        Config = new AsciiViaTcpConfig();
                    }
                    break;
                case ConfigType.Rtu:
                    if (RtuConfig == null)
                    {
                        Config = new RtuConfig();
                    }
                    break;
                case ConfigType.RtuViaTcp:
                    if (RtuViaTcpConfig == null)
                    {
                        Config = new RTUviaTCPConfig();
                    }
                    break;
                case ConfigType.Tcp:
                    if (TcpConfig == null)
                    {
                        Config = new TcpConfig();
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
                return new[]
                    {
                        75, 110, 300, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200
                    };
            }
        }

        public int[] DataBits
        {
            get
            {
                return new[]
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
                var assemblyLocation = Assembly.GetAssembly(typeof(ModbusOptionsEditorViewModel)).Location;
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

        protected virtual void OnPropertyChanged(Expression<Func<ModbusOptionsEditorViewModel, object>> getPropertyExpresstion)
        {
            var property = PropertyHelper.GetPropertyName(this, getPropertyExpresstion);
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(property));
        }
    }
}
