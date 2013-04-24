using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiResources.Loc;
using PHmiModel;

namespace PHmiConfigurator.Modules.Collection
{
    public class IoDevicesViewModel : CollectionViewModel<io_devices, io_devices.IoDevicesMetadata>
    {
        public IoDevicesViewModel(): base(null)
        {
        }

        public override string Name
        {
            get { return Res.IoDevices; }
        }

        protected override IEditDialog<io_devices.IoDevicesMetadata> CreateAddDialog()
        {
            return new EditIoDevice { Title = Res.AddIoDevice, Owner = Window.GetWindow(View) };
        }

        protected override IEditDialog<io_devices.IoDevicesMetadata> CreateEditDialog()
        {
            return new EditIoDevice { Title = Res.EditIoDevice, Owner = Window.GetWindow(View) };
        }

        protected override string[] GetCopyData(io_devices item)
        {
            return new []
                {
                    item.type,
                    item.options
                };
        }

        protected override string[] GetCopyHeaders()
        {
            return new []
                {
                    ReflectionHelper.GetDisplayName<io_devices>(d => d.type),
                    ReflectionHelper.GetDisplayName<io_devices>(d => d.options)
                };
        }

        protected override void SetCopyData(io_devices item, string[] data)
        {
            item.type = data[0];
            item.options = data[1];
        }
    }
}
