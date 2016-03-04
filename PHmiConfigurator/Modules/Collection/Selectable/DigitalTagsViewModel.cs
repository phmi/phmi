using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel;
using PHmiModel.Entities;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection.Selectable
{
    public class DigitalTagsViewModel : SelectableCollectionViewModel<DigTag, DigTag.DigTagMetadata, PHmiModel.Entities.IoDevice>
    {
        public DigitalTagsViewModel() : base(null) { }

        public override string Name
        {
            get { return Res.DigitalTags; }
        }
        
        public override string Error
        {
            get
            {
                var error = base.Error;
                if (CurrentSelector != null)
                {
                    var ioDeviceId = CurrentSelector.Id;
                    var numericTagsNames = Context.Get<NumTag>()
                        .Where(t => t.RefIoDevice == ioDeviceId).Select(t => t.Name).Distinct().ToDictionary(n => n);
                    var names = List.Where(t => numericTagsNames.ContainsKey(t.Name)).Select(t => t.Name).ToArray();
                    if (names.Any())
                    {
                        if (!string.IsNullOrEmpty(error))
                            error += Environment.NewLine;
                        error += string.Format(Res.NumericTagPresentMessage, ReflectionHelper.GetDisplayName<DigTag>(t => t.Name))
                            + Environment.NewLine
                            + string.Join(", ", names) + ".";
                    }
                }
                return error;
            }
        }

        protected override IEditDialog<DigTag.DigTagMetadata> CreateAddDialog()
        {
            return new EditDigitalTag { Title = Res.AddDigitalTag, Owner = Window.GetWindow(View) };
        }

        protected override IEditDialog<DigTag.DigTagMetadata> CreateEditDialog()
        {
            return new EditDigitalTag { Title = Res.EditDigitalTag, Owner = Window.GetWindow(View) };
        }

        protected override string[] GetCopyData(DigTag item)
        {
            return new []
                {
                    item.Device,
                    item.Description,
                    item.CanRead.ToString(CultureInfo.InvariantCulture)
                };
        }

        protected override string[] GetCopyHeaders()
        {
            return new []
                {
                    ReflectionHelper.GetDisplayName<DigTag>(t => t.Device),
                    ReflectionHelper.GetDisplayName<DigTag>(t => t.Description),
                    ReflectionHelper.GetDisplayName<DigTag>(t => t.CanRead)
                };
        }

        protected override void SetCopyData(DigTag item, string[] data)
        {
            item.Device = data[0];
            item.Description = data[1];
            item.CanRead = bool.Parse(data[2]);
        }
    }
}
