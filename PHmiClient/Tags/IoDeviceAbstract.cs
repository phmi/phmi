using System;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClient.Tags
{
    public abstract class IoDeviceAbstract
    {
        protected abstract IDigitalTag AddDigitalTag(int id, string name, Func<string> descriptionGetter);
        protected abstract INumericTag AddNumericTag(int id, string name, Func<string> descriptionGetter,
            Func<string> formatGetter, Func<string> engUnitGetter, double? minValue, double? maxValue);
        internal abstract RemapTagsParameter CreateRemapParameter();
        internal abstract void ApplyRemapResult(RemapTagsResult result);
        public abstract string Name { get; }
    }
}
