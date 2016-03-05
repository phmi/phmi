using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using PHmiClient.Utils;
using PHmiConfigurator.Dialogs;
using PHmiModel;
using PHmiModel.Entities;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection.Selectable
{
    public class NumericTagsViewModel : SelectableCollectionViewModel<NumTag, NumTag.NumTagMetadata, PHmiModel.Entities.IoDevice>
    {
        private IEnumerable<NumTagType> _numTagTypes;

        public NumericTagsViewModel() : base(null)
        {
        }

        public IEnumerable<NumTagType> NumTagTypes
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
                    var ioDeviceId = CurrentSelector.Id;
                    var digitalTagsNames = Context.Get<DigTag>()
                        .Where(t => t.RefIoDevice == ioDeviceId).Select(t => t.Name).Distinct().ToDictionary(n => n);
                    var names = List.Where(t => digitalTagsNames.ContainsKey(t.Name)).Select(t => t.Name).ToArray();
                    if (names.Any())
                    {
                        if (!string.IsNullOrEmpty(error))
                            error += Environment.NewLine;
                        error += string.Format(Res.DigitalTagPresentMessage, ReflectionHelper.GetDisplayName<DigTag>(t => t.Name))
                            + Environment.NewLine
                            + string.Join(", ", names) + ".";
                    }
                }
                return error;
            }
        }

        protected override void PostReloadAction()
        {
            NumTagTypes = Context.Get<NumTagType>().OrderBy(t => t.Id).ToArray();
            base.PostReloadAction();
        }

        protected override IEditDialog<NumTag.NumTagMetadata> CreateAddDialog()
        {
            return new EditNumericTag
                {
                    NumTagTypes = NumTagTypes,
                    Title = Res.AddNumericTag,
                    Owner = Window.GetWindow(View)
                };
        }

        protected override IEditDialog<NumTag.NumTagMetadata> CreateEditDialog()
        {
            return new EditNumericTag
                {
                    NumTagTypes = NumTagTypes,
                    Title = Res.EditNumericTag,
                    Owner = Window.GetWindow(View)
                };
        }

        protected override string[] GetCopyData(NumTag item)
        {
            return new[]
                {
                    item.Device,
                    item.Description,
                    item.CanRead.ToString(CultureInfo.InvariantCulture),
                    item.NumTagType.Name,
                    item.Format,
                    item.EngUnit,
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
                    ReflectionHelper.GetDisplayName<NumTag>(t => t.Device),
                    ReflectionHelper.GetDisplayName<NumTag>(t => t.Description),
                    ReflectionHelper.GetDisplayName<NumTag>(t => t.CanRead),
                    ReflectionHelper.GetDisplayName<NumTag>(t => t.NumTagType),
                    ReflectionHelper.GetDisplayName<NumTag>(t => t.Format),
                    ReflectionHelper.GetDisplayName<NumTag>(t => t.EngUnit),
                    ReflectionHelper.GetDisplayName<NumTag>(t => t.RawMin),
                    ReflectionHelper.GetDisplayName<NumTag>(t => t.RawMax),
                    ReflectionHelper.GetDisplayName<NumTag>(t => t.EngMin),
                    ReflectionHelper.GetDisplayName<NumTag>(t => t.EngMax)
                };
        }

        protected override void SetCopyData(NumTag item, string[] data)
        {
            item.Device = data[0];
            item.Description = data[1];
            item.CanRead = bool.Parse(data[2]);
            item.NumTagType = NumTagTypes.First(t => t.Name == data[3]);
            item.Format = data[4];
            item.EngUnit = data[5];
            item.RawMin = data[6];
            item.RawMax = data[7];
            item.EngMin = data[8];
            item.EngMax = data[9];
        }
    }
}
