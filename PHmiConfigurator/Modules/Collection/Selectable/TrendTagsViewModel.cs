using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PHmiConfigurator.Dialogs;
using PHmiModel;
using PHmiResources.Loc;
using PHmiTools;

namespace PHmiConfigurator.Modules.Collection.Selectable
{
    public class TrendTagsViewModel : SelectableCollectionViewModel<trend_tags, trend_tags.TrendTagsMetadata, trend_categories>
    {
        public TrendTagsViewModel() : base(null)
        {
        }

        public override string Name
        {
            get { return Res.TrendTags; }
        }

        public override string Error
        {
            get
            {
                var error = base.Error;
                if (List.Count > PHmiConstants.MaxTrendTagsInCategory)
                {
                    if (!string.IsNullOrEmpty(error))
                        error += Environment.NewLine;
                    error += string.Format(Res.TooManyTrendTagsInCategoryMessage, PHmiConstants.MaxTrendTagsInCategory) + ".";
                }
                return error;
            }
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

        #region NumericTags

        private Dictionary<string, num_tags> _numericTagsDictionary; 

        private IEnumerable<num_tags> _numericTags;
 
        public IEnumerable<num_tags> NumericTags
        {
            get { return _numericTags; }
            set
            {
                _numericTags = value;
                OnPropertyChanged(this, v => v.NumericTags);
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

            var numericTags = Context.Get<num_tags>().OrderBy(t => t.io_devices.name).ThenBy(t => t.name).ToArray();
            NumericTags = numericTags;
            _numericTagsDictionary = new Dictionary<string, num_tags>(numericTags.Length);
            foreach (var tag in numericTags)
            {
                var key = tag.io_devices.name + "." + tag.name;
                if (!_numericTagsDictionary.ContainsKey(key))
                    _numericTagsDictionary.Add(key, tag);
            }

            base.PostReloadAction();
        }

        protected override IEditDialog<trend_tags.TrendTagsMetadata> CreateAddDialog()
        {
            return new EditTrendTag
                {
                    Title = Res.AddTrendTag,
                    Owner = Window.GetWindow(View),
                    DigitalTags = DigitalTags,
                    NumericTags = NumericTags
                };
        }

        protected override IEditDialog<trend_tags.TrendTagsMetadata> CreateEditDialog()
        {
            return new EditTrendTag
                {
                    Title = Res.EditTrendTag,
                    Owner = Window.GetWindow(View),
                    DigitalTags = DigitalTags,
                    NumericTags = NumericTags
                };
        }

        protected override string[] GetCopyData(trend_tags item)
        {
            return new []
                {
                    item.num_tags.io_devices.name,
                    item.num_tags.name,
                    item.dig_tags == null ? string.Empty : item.dig_tags.io_devices.name,
                    item.dig_tags == null ? string.Empty : item.dig_tags.name,
                    item.description
                };
        }

        protected override string[] GetCopyHeaders()
        {
            return new[]
                {
                    Res.IoDevice,
                    Res.NumericTag,
                    Res.TriggerIoDevice,
                    Res.Trigger,
                    Res.Description
                };
        }

        protected override void SetCopyData(trend_tags item, string[] data)
        {
            item.num_tags = _numericTagsDictionary[data[0] + "." + data[1]];
            dig_tags digTag;
            item.dig_tags = _digitalTagsDictionary.TryGetValue(data[2] + "." + data[3], out digTag) ? digTag : null;
            item.description = data[4];
        }
    }
}
