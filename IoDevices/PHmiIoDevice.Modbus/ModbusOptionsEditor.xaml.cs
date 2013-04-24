using PHmiClient.Utils;
using PHmiIoDevice.Modbus.Configuration;
using PHmiIoDeviceTools;
using System;

namespace PHmiIoDevice.Modbus
{
    /// <summary>
    /// Interaction logic for ModbusOptionsEditor.xaml
    /// </summary>
    public partial class ModbusOptionsEditor : IOptionsEditor
    {
        private readonly ModbusOptionsEditorViewModel _viewModel = new ModbusOptionsEditorViewModel();

        public ModbusOptionsEditor()
        {
            InitializeComponent();
            ViewModel.ConfigChanged += (sender, args) => OnConfigChanged();
        }

        private void OnConfigChanged()
        {
            EventHelper.Raise(ref OptionsChanged, this, EventArgs.Empty);
        }

        public ModbusOptionsEditorViewModel ViewModel { get { return _viewModel; } }

        public void SetOptions(string options)
        {
            try
            {
                ViewModel.Config = ConfigHelper.GetConfig(options);
            }
            catch
            {
                ViewModel.Config = new TcpConfig();
            }
        }

        public string GetOptions()
        {
            return ViewModel.Config.GetXml();
        }

        public event EventHandler OptionsChanged;
    }
}
