using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection.Selectable
{
    public class DigitalTagsViewModel : SelectableCollectionViewModel<dig_tags, dig_tags.DigTagsMetadata, io_devices>
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
                    var ioDeviceId = CurrentSelector.id;
                    var numericTagsNames = Context.Get<num_tags>()
                        .Where(t => t.ref_io_devices == ioDeviceId).Select(t => t.name).Distinct().ToDictionary(n => n);
                    var names = List.Where(t => numericTagsNames.ContainsKey(t.name)).Select(t => t.name).ToArray();
                    if (names.Any())
                    {
                        if (!string.IsNullOrEmpty(error))
                            error += Environment.NewLine;
                        error += string.Format(Res.NumericTagPresentMessage, ReflectionHelper.GetDisplayName<dig_tags>(t => t.name))
                            + Environment.NewLine
                            + string.Join(", ", names) + ".";
                    }
                }
                return error;
            }
        }

        protected override IEditDialog<dig_tags.DigTagsMetadata> CreateAddDialog()
        {
            return new EditDigitalTag { Title = Res.AddDigitalTag, Owner = Window.GetWindow(View) };
        }

        protected override IEditDialog<dig_tags.DigTagsMetadata> CreateEditDialog()
        {
            return new EditDigitalTag { Title = Res.EditDigitalTag, Owner = Window.GetWindow(View) };
        }

        protected override string[] GetCopyData(dig_tags item)
        {
            return new []
                {
                    item.device,
                    item.description,
                    item.can_read.ToString(CultureInfo.InvariantCulture)
                };
        }

        protected override string[] GetCopyHeaders()
        {
            return new []
                {
                    ReflectionHelper.GetDisplayName<dig_tags>(t => t.device),
                    ReflectionHelper.GetDisplayName<dig_tags>(t => t.description),
                    ReflectionHelper.GetDisplayName<dig_tags>(t => t.can_read)
                };
        }

        protected override void SetCopyData(dig_tags item, string[] data)
        {
            item.device = data[0];
            item.description = data[1];
            item.can_read = bool.Parse(data[2]);
        }
    }
}
