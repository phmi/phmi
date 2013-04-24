using System;
using PHmiModel;

namespace PHmiRunner.Utils.IoDeviceRunner
{
    public class NumTagValueConverter
    {
        private readonly double _add;
        private readonly double _div;
        private readonly double _mul;
        private readonly bool _needCalculation;
        private readonly Type _type;

        public NumTagValueConverter(num_tags numericTag)
        {
            _type = numericTag.TagType;
            if (numericTag.raw_min == null || numericTag.raw_max == null || numericTag.eng_min == null ||
                numericTag.eng_max == null || numericTag.raw_max - numericTag.raw_min == 0)
                return;
            _needCalculation = true;
            _mul = (double) numericTag.eng_max - (double) numericTag.eng_min;
            _add = (double) numericTag.eng_min*(double) numericTag.raw_max -
                   (double) numericTag.eng_max*(double) numericTag.raw_min;
            _div = (double) numericTag.raw_max - (double) numericTag.raw_min;
        }

        public double? RawToEng(dynamic rawValue)
        {
            if (rawValue == null)
                return null;
            if (_needCalculation)
            {
                return (rawValue * _mul + _add) / _div;
            }
            return rawValue;
        }

        public object EngToRaw(double engValue)
        {
            return _needCalculation
                           ? Convert.ChangeType((engValue * _div - _add) / _mul, _type)
                           : Convert.ChangeType(engValue, _type);
        }
    }
}
