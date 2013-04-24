using System;
using System.Collections.Generic;
using System.Linq;
using PHmiClient.Utils;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClient.Tags
{
    public class IoDeviceBase : IoDeviceAbstract
    {
        private readonly IDispatcherService _dispatcherService;
        private readonly int _id;
        private readonly string _name;
        private readonly IList<ITag> _tags = new List<ITag>();
        private readonly IList<TagAbstract<bool?>> _digitalTags = new List<TagAbstract<bool?>>();
        private readonly IList<TagAbstract<bool?>> _readDigitalTags = new List<TagAbstract<bool?>>();
        private readonly IList<TagAbstract<double?>> _numericTags = new List<TagAbstract<double?>>();
        private readonly IList<TagAbstract<double?>> _readNumericTags = new List<TagAbstract<double?>>();
        
        protected IoDeviceBase(int id, string name)
        {
            _dispatcherService = new DispatcherService();
            _id = id;
            _name = name;
        }

        protected void Add(TagAbstract<bool?> digitalTag)
        {
            _tags.Add(digitalTag);
            _digitalTags.Add(digitalTag);
        }

        protected override IDigitalTag AddDigitalTag(int id, string name, Func<string> descriptionGetter)
        {
            var digitalTag = new DigitalTag(_dispatcherService, id, name, descriptionGetter);
            Add(digitalTag);
            return digitalTag;
        }

        protected void Add(TagAbstract<double?> numericTag)
        {
            _tags.Add(numericTag);
            _numericTags.Add(numericTag);
        }

        protected override INumericTag AddNumericTag(int id, string name, Func<string> descriptionGetter,
            Func<string> formatGetter, Func<string> engUnitGetter, double? minValue, double? maxValue)
        {
            var numericTag = new NumericTag(
                _dispatcherService, id, name, descriptionGetter, formatGetter, engUnitGetter, minValue, maxValue);
            Add(numericTag);
            return numericTag;
        }

        internal override RemapTagsParameter CreateRemapParameter()
        {
            var parameter = new RemapTagsParameter { IoDeviceId = _id };
            UpdateDigReadIds(parameter);
            UpdateNumReadIds(parameter);
            UpdateDigWriteData(parameter);
            UpdateNumWriteData(parameter);
            if (parameter.DigReadIds.Any()
                || parameter.NumReadIds.Any()
                || parameter.DigWriteIds.Any()
                || parameter.NumWriteIds.Any())
                return parameter;
            return null;
        }

        private void UpdateDigWriteData(RemapTagsParameter parameter)
        {
            var ids = new List<int>();
            var values = new List<bool>();
            foreach (var tag in _digitalTags)
            {
                if (!tag.IsWritten)
                    continue;
                var value = tag.GetWrittenValue();
                if (!value.HasValue)
                    continue;
                ids.Add(tag.Id);
                values.Add(value.Value);
            }
            parameter.DigWriteIds = ids.ToArray();
            parameter.DigWriteValues = values.ToArray();
        }

        private void UpdateNumWriteData(RemapTagsParameter parameter)
        {
            var ids = new List<int>();
            var values = new List<double>();
            foreach (var tag in _numericTags)
            {
                if (!tag.IsWritten)
                    continue;
                var value = tag.GetWrittenValue();
                if (!value.HasValue)
                    continue;
                ids.Add(tag.Id);
                values.Add(value.Value);
            }
            parameter.NumWriteIds = ids.ToArray();
            parameter.NumWriteValues = values.ToArray();
        }

        private void UpdateDigReadIds(RemapTagsParameter parameter)
        {
            _readDigitalTags.Clear();
            var unReadTags = new List<TagAbstract<bool?>>();
            foreach (var tag in _digitalTags)
            {
                if (tag.IsRead)
                    _readDigitalTags.Add(tag);
                else
                    unReadTags.Add(tag);
            }
            foreach (var tag in unReadTags)
            {
                tag.UpdateValue(null);
            }
            parameter.DigReadIds = _readDigitalTags.Select(t => t.Id).ToArray();
        }

        private void UpdateNumReadIds(RemapTagsParameter parameter)
        {
            _readNumericTags.Clear();
            var unReadTags = new List<TagAbstract<double?>>(); 
            foreach (var tag in _numericTags)
            {
                if (tag.IsRead)
                    _readNumericTags.Add(tag);
                else
                    unReadTags.Add(tag);
            }
            foreach (var tag in unReadTags)
            {
                tag.UpdateValue(null);
            }
            parameter.NumReadIds = _readNumericTags.Select(t => t.Id).ToArray();
        }

        internal override void ApplyRemapResult(RemapTagsResult result)
        {
            if (result != null)
            {
                ApplyForDigitalTags(result);
                ApplyForNumericTags(result);
            }
            else
            {
                ApplyForDigitalTagsNull();
                ApplyForNumericTagsNull();
            }
        }

        public override string Name
        {
            get { return _name; }
        }

        private void ApplyForDigitalTags(RemapTagsResult result)
        {
            for (var i = 0; i < _readDigitalTags.Count; i++)
            {
                var tag = _readDigitalTags[i];
                tag.UpdateValue(result.DigReadValues[i]);
            }
        }

        private void ApplyForNumericTags(RemapTagsResult result)
        {
            for (var i = 0; i < _readNumericTags.Count; i++)
            {
                var tag = _readNumericTags[i];
                tag.UpdateValue(result.NumReadValues[i]);
            }
        }

        private void ApplyForDigitalTagsNull()
        {
            foreach (var tag in _readDigitalTags)
            {
                tag.UpdateValue(null);
            }
        }

        private void ApplyForNumericTagsNull()
        {
            foreach (var tag in _readNumericTags)
            {
                tag.UpdateValue(null);
            }
        }
    }
}
