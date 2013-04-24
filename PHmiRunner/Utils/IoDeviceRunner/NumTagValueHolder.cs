using System;
using PHmiModel;

namespace PHmiRunner.Utils.IoDeviceRunner
{
    public class NumTagValueHolder : TagValueHolder<double?>
    {
        private readonly NumTagValueConverter _converter;
        private readonly string _address;

        public NumTagValueHolder(num_tags tag)
        {
            _converter = new NumTagValueConverter(tag);
            _address = tag.device;
        }

        public override string Address { get { return _address; } }

        protected override double? RawToEng(object value)
        {
            return _converter.RawToEng(value);
        }

        protected override object EngToRaw(double? value)
        {
            if (!value.HasValue)
                throw new Exception("Value is null");
            return _converter.EngToRaw(value.Value);
        }
    }
}
