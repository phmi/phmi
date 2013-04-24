using PHmiClient.Utils;
using PHmiIoDevice.Melsec.Configuration;
using PHmiIoDeviceTools;
using System;

namespace PHmiIoDevice.Melsec
{
    /// <summary>
    /// Interaction logic for MelsecOptionsEditor.xaml
    /// </summary>
    public partial class MelsecOptionsEditor : IOptionsEditor
    {
        private readonly MelsecOptionsEditorViewModel _viewModel = new MelsecOptionsEditorViewModel();

        public MelsecOptionsEditor()
        {
            InitializeComponent();
            ViewModel.ConfigChanged += (sender, args) => OnConfigChanged();
        }

        private void OnConfigChanged()
        {
            EventHelper.Raise(ref OptionsChanged, this, EventArgs.Empty);
        }

        public MelsecOptionsEditorViewModel ViewModel { get { return _viewModel; } }

        public void SetOptions(string options)
        {
            try
            {
                ViewModel.Config = ConfigHelper.GetConfig(options);
            }
            catch
            {
                ViewModel.Config = new QConfig();
            }
        }

        public string GetOptions()
        {
            return ViewModel.Config.GetXml();
        }

        public event EventHandler OptionsChanged;
    }
}
