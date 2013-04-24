using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection.Selectable
{
    public class NumericTagsViewModel : SelectableCollectionViewModel<num_tags, num_tags.NumTagsMetadata, io_devices>
    {
        private IEnumerable<num_tag_types> _numTagTypes;

        public NumericTagsViewModel() : base(null)
        {
        }

        public IEnumerable<num_tag_types> NumTagTypes
        {
            get { return _numTagTypes; }
            set
            {
                _numTagTypes = value;
                OnPropertyChanged(this, v => v.NumTagTypes);
            }
        }

        public override string Name
        {
            get { return Res.NumericTags; }
        }

        public override string Error
        {
            get
            {
                var error = base.Error;
                if (CurrentSelector != null)
                {
                    var ioDeviceId = CurrentSelector.id;
                    var digitalTagsNames = Context.Get<dig_tags>()
                        .Where(t => t.ref_io_devices == ioDeviceId).Select(t => t.name).Distinct().ToDictionary(n => n);
                    var names = List.Where(t => digitalTagsNames.ContainsKey(t.name)).Select(t => t.name).ToArray();
                    if (names.Any())
                    {
                        if (!string.IsNullOrEmpty(error))
                            error += Environment.NewLine;
                        error += string.Format(Res.DigitalTagPresentMessage, ReflectionHelper.GetDisplayName<dig_tags>(t => t.name))
                            + Environment.NewLine
                            + string.Join(", ", names) + ".";
                    }
                }
                return error;
            }
        }

        protected override void PostReloadAction()
        {
            NumTagTypes = Context.Get<num_tag_types>().OrderBy(t => t.id).ToArray();
            base.PostReloadAction();
        }

        protected override IEditDialog<num_tags.NumTagsMetadata> CreateAddDialog()
        {
            return new EditNumericTag
                {
                    NumTagTypes = NumTagTypes,
                    Title = Res.AddNumericTag,
                    Owner = Window.GetWindow(View)
                };
        }

        protected override IEditDialog<num_tags.NumTagsMetadata> CreateEditDialog()
        {
            return new EditNumericTag
                {
                    NumTagTypes = NumTagTypes,
                    Title = Res.EditNumericTag,
                    Owner = Window.GetWindow(View)
                };
        }

        protected override string[] GetCopyData(num_tags item)
        {
            return new[]
                {
                    item.device,
                    item.description,
                    item.can_read.ToString(CultureInfo.InvariantCulture),
                    item.num_tag_types.name,
                    item.format,
                    item.eng_unit,
                    item.RawMin,
                    item.RawMax,
                    item.EngMin,
                    item.EngMax
                };
        }

        protected override string[] GetCopyHeaders()
        {
            return new[]
                {
                    ReflectionHelper.GetDisplayName<num_tags>(t => t.device),
                    ReflectionHelper.GetDisplayName<num_tags>(t => t.description),
                    ReflectionHelper.GetDisplayName<num_tags>(t => t.can_read),
                    ReflectionHelper.GetDisplayName<num_tags>(t => t.num_tag_types),
                    ReflectionHelper.GetDisplayName<num_tags>(t => t.format),
                    ReflectionHelper.GetDisplayName<num_tags>(t => t.eng_unit),
                    ReflectionHelper.GetDisplayName<num_tags>(t => t.RawMin),
                    ReflectionHelper.GetDisplayName<num_tags>(t => t.RawMax),
                    ReflectionHelper.GetDisplayName<num_tags>(t => t.EngMin),
                    ReflectionHelper.GetDisplayName<num_tags>(t => t.EngMax)
                };
        }

        protected override void SetCopyData(num_tags item, string[] data)
        {
            item.device = data[0];
            item.description = data[1];
            item.can_read = bool.Parse(data[2]);
            item.num_tag_types = NumTagTypes.First(t => t.name == data[3]);
            item.format = data[4];
            item.eng_unit = data[5];
            item.RawMin = data[6];
            item.RawMax = data[7];
            item.EngMin = data[8];
            item.EngMax = data[9];
        }
    }
}
