using System;
using PHmiModel;

namespace PHmiRunner.Utils.IoDeviceRunner
{
    public class DigTagValueHolder : TagValueHolder<bool?>
    {
        private readonly string _address;

        public DigTagValueHolder(dig_tags tag)
        {
            _address = tag.device;
        }

        public override string Address
        {
            get { return _address; }
        }

        protected override bool? RawToEng(object value)
        {
            return value as bool?;
        }

        protected override object EngToRaw(bool? value)
        {
            if (!value.HasValue)
                throw new Exception("Value is null");
            return value.Value;
        }
    }
}
