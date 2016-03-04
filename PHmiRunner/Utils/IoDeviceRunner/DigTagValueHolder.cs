using System;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiRunner.Utils.IoDeviceRunner
{
    public class DigTagValueHolder : TagValueHolder<bool?>
    {
        private readonly string _address;

        public DigTagValueHolder(DigTag tag)
        {
            _address = tag.Device;
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
