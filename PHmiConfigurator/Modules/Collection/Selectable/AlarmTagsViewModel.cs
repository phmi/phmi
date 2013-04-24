using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using PHmiClient.Converters;
using PHmiConfigurator.Dialogs;
using PHmiModel;
using PHmiResources.Loc;

namespace PHmiConfigurator.Modules.Collection.Selectable
{
    public class AlarmTagsViewModel : SelectableCollectionViewModel<alarm_tags, alarm_tags.AlarmTagsMetadata, alarm_categories>
    {
        public AlarmTagsViewModel() : base(null)
        {
        }

        public override string Name
        {
            get { return Res.AlarmTags; }
        }

        #region DigitalTags

        private Dictionary<string, dig_tags> _digitalTagsDictionary; 

        private IEnumerable<dig_tags> _digitalTags;

        public IEnumerable<dig_tags> DigitalTags
        {
            get { return _digitalTags; }
            set
            {
                _digitalTags = value;
                OnPropertyChanged(this, v => v.DigitalTags);
            }
        }

        #endregion

        protected override void PostReloadAction()
        {
            var digitalTags = Context.Get<dig_tags>().OrderBy(t => t.io_devices.name).ThenBy(t => t.name).ToArray();
            DigitalTags = digitalTags;
            _digitalTagsDictionary = new Dictionary<string, dig_tags>(digitalTags.Length);
            foreach (var tag in digitalTags)
            {
                var key = tag.io_devices.name + "." + tag.name;
                if (!_digitalTagsDictionary.ContainsKey(key))
                    _digitalTagsDictionary.Add(key, tag);
            }
            base.PostReloadAction();
        }

        protected override IEditDialog<alarm_tags.AlarmTagsMetadata> CreateAddDialog()
        {
            return new EditAlarmTag
                {
                    Title = Res.AddAlarmTag,
                    Owner = Window.GetWindow(View),
                    DigitalTags = DigitalTags
                };
        }

        protected override IEditDialog<alarm_tags.AlarmTagsMetadata> CreateEditDialog()
        {
            return new EditAlarmTag
                {
                    Title = Res.EditAlarmTag, 
                    Owner = Window.GetWindow(View),
                    DigitalTags = DigitalTags
                };
        }

        protected override string[] GetCopyData(alarm_tags item)
        {
            return new []
                {
                    item.dig_tags.io_devices.name,
                    item.dig_tags.name,
                    item.location,
                    item.description,
                    item.acknowledgeable.ToString(CultureInfo.InvariantCulture),
                    Int32ToPrivilegedConverter.Convert(item.privilege)
                };
        }

        protected override string[] GetCopyHeaders()
        {
            return new []
                {
                    Res.IoDevice,
                    Res.DigitalTag,
                    Res.Location,
                    Res.Description,
                    Res.Acknowledgeable,
                    Res.Privilege
                };
        }

        protected override void SetCopyData(alarm_tags item, string[] data)
        {
            item.dig_tags = _digitalTagsDictionary[data[0] + "." + data[1]];
            item.location = data[2];
            item.description = data[3];
            item.acknowledgeable = bool.Parse(data[4]);
            item.privilege = Int32ToPrivilegedConverter.ConvertBack(data[5]);
        }
    }
}
