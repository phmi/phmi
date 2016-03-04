using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiResources.Loc;
using PHmiModel;

namespace PHmiConfigurator.Modules.Collection
{
    public class IoDevicesViewModel : CollectionViewModel<PHmiModel.Entities.IoDevice, PHmiModel.Entities.IoDevice.IoDeviceMetadata>
    {
        public IoDevicesViewModel(): base(null)
        {
        }

        public override string Name
        {
            get { return Res.IoDevices; }
        }

        protected override IEditDialog<PHmiModel.Entities.IoDevice.IoDeviceMetadata> CreateAddDialog()
        {
            return new EditIoDevice { Title = Res.AddIoDevice, Owner = Window.GetWindow(View) };
        }

        protected override IEditDialog<PHmiModel.Entities.IoDevice.IoDeviceMetadata> CreateEditDialog()
        {
            return new EditIoDevice { Title = Res.EditIoDevice, Owner = Window.GetWindow(View) };
        }

        protected override string[] GetCopyData(PHmiModel.Entities.IoDevice item)
        {
            return new []
                {
                    item.Type,
                    item.Options
                };
        }

        protected override string[] GetCopyHeaders()
        {
            return new []
                {
                    ReflectionHelper.GetDisplayName<PHmiModel.Entities.IoDevice>(d => d.Type),
                    ReflectionHelper.GetDisplayName<PHmiModel.Entities.IoDevice>(d => d.Options)
                };
        }

        protected override void SetCopyData(PHmiModel.Entities.IoDevice item, string[] data)
        {
            item.Type = data[0];
            item.Options = data[1];
        }
    }
}
