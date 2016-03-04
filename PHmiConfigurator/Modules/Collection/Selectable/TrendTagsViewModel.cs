using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PHmiConfigurator.Dialogs;
using PHmiModel;
using PHmiModel.Entities;
using PHmiResources.Loc;
using PHmiTools;

namespace PHmiConfigurator.Modules.Collection.Selectable
{
    public class TrendTagsViewModel : SelectableCollectionViewModel<PHmiModel.Entities.TrendTag, PHmiModel.Entities.TrendTag.TrendTagMetadata, PHmiModel.Entities.TrendCategory>
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

        private Dictionary<string, DigTag> _digitalTagsDictionary; 

        private IEnumerable<DigTag> _digitalTags;

        public IEnumerable<DigTag> DigitalTags
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

        private Dictionary<string, NumTag> _numericTagsDictionary; 

        private IEnumerable<NumTag> _numericTags;
 
        public IEnumerable<NumTag> NumericTags
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
            var digitalTags = Context.Get<DigTag>().OrderBy(t => t.IoDevice.Name).ThenBy(t => t.Name).ToArray();
            DigitalTags = digitalTags;
            _digitalTagsDictionary = new Dictionary<string, DigTag>(digitalTags.Length);
            foreach (var tag in digitalTags)
            {
                var key = tag.IoDevice.Name + "." + tag.Name;
                if (!_digitalTagsDictionary.ContainsKey(key))
                    _digitalTagsDictionary.Add(key, tag);
            }

            var numericTags = Context.Get<NumTag>().OrderBy(t => t.IoDevice.Name).ThenBy(t => t.Name).ToArray();
            NumericTags = numericTags;
            _numericTagsDictionary = new Dictionary<string, NumTag>(numericTags.Length);
            foreach (var tag in numericTags)
            {
                var key = tag.IoDevice.Name + "." + tag.Name;
                if (!_numericTagsDictionary.ContainsKey(key))
                    _numericTagsDictionary.Add(key, tag);
            }

            base.PostReloadAction();
        }

        protected override IEditDialog<PHmiModel.Entities.TrendTag.TrendTagMetadata> CreateAddDialog()
        {
            return new EditTrendTag
                {
                    Title = Res.AddTrendTag,
                    Owner = Window.GetWindow(View),
                    DigitalTags = DigitalTags,
                    NumericTags = NumericTags
                };
        }

        protected override IEditDialog<PHmiModel.Entities.TrendTag.TrendTagMetadata> CreateEditDialog()
        {
            return new EditTrendTag
                {
                    Title = Res.EditTrendTag,
                    Owner = Window.GetWindow(View),
                    DigitalTags = DigitalTags,
                    NumericTags = NumericTags
                };
        }

        protected override string[] GetCopyData(PHmiModel.Entities.TrendTag item)
        {
            return new []
                {
                    item.NumTag.IoDevice.Name,
                    item.NumTag.Name,
                    item.Trigger == null ? string.Empty : item.Trigger.IoDevice.Name,
                    item.Trigger == null ? string.Empty : item.Trigger.Name,
                    item.Description
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

        protected override void SetCopyData(PHmiModel.Entities.TrendTag item, string[] data)
        {
            item.NumTag = _numericTagsDictionary[data[0] + "." + data[1]];
            DigTag digTag;
            item.Trigger = _digitalTagsDictionary.TryGetValue(data[2] + "." + data[3], out digTag) ? digTag : null;
            item.Description = data[4];
        }
    }
}
