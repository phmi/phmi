using System;
using System.Windows;
using PHmiClient.Utils;
using PHmiClient.Utils.Configuration;
using PHmiIoDeviceTools;
using PHmiModel;
using PHmiTools.Controls;

namespace PHmiConfigurator.Dialogs
{
    /// <summary>
    /// Interaction logic for EditIoDevice.xaml
    /// </summary>
    public partial class EditIoDevice : IEditDialog<io_devices.IoDevicesMetadata>
    {
        public EditIoDevice()
        {
            this.UpdateLanguage();
            InitializeComponent();
            WindowPositionSettings.UseWith(this, "EditIoDeviceWindowPosition");
            ViewModel.View = this;
            tbName.Focus();
            UpdateTypeEditorRowHeight();
            Closing += EditIoDeviceClosing;
        }

        private void EditIoDeviceClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveTypeEditorRowHeight();
        }

        public EditIoDeviceViewModel ViewModel
        {
            get { return (EditIoDeviceViewModel) Resources["ViewModel"]; }
        }

        public io_devices.IoDevicesMetadata Entity
        {
            get { return ViewModel.Entity; }
            set { ViewModel.Entity = value; }
        }

        private void IoDeviceTypeEditorSelectedItemChanged(object sender, EventArgs e)
        {
            var oldEditor = svOptionsEditorHolder.Content as IOptionsEditor;
            if (oldEditor != null)
            {
                oldEditor.OptionsChanged -= OptionsEditorOptionsChanged;
            }
            var editor = (IoDeviceTypeEditor) sender;
            var optionsEditor = editor.CreateOptionsEditor();
            if (optionsEditor == null)
            {
                tbOptions.Visibility = Visibility.Visible;
                svOptionsEditorHolder.Content = null;
                svOptionsEditorHolder.Visibility = Visibility.Collapsed;
            }
            else
            {
                tbOptions.Visibility = Visibility.Collapsed;
                svOptionsEditorHolder.Content = optionsEditor;
                optionsEditor.OptionsChanged += OptionsEditorOptionsChanged;
                optionsEditor.SetOptions(Entity.options);
                svOptionsEditorHolder.Visibility = Visibility.Visible;
            }
        }

        private void OptionsEditorOptionsChanged(object sender, EventArgs e)
        {
            var editor = (IOptionsEditor) sender;
            Entity.options = editor.GetOptions();
        }

        private const string EditIoDeviceWindowTypeEditor = "EditIoDeviceWindowTypeEditor";

        private const string RowHeight = "RowHeight";

        private void UpdateTypeEditorRowHeight()
        {
            var settings = new Settings(EditIoDeviceWindowTypeEditor);
            var height = settings.GetDouble(RowHeight);
            if (height.HasValue)
                typeEditorRow.Height = new GridLength(height.Value);
        }

        private void SaveTypeEditorRowHeight()
        {
            var settings = new Settings(EditIoDeviceWindowTypeEditor);
            settings.SetDouble(RowHeight, typeEditorRow.ActualHeight);
            settings.Save();
        }
    }
}
