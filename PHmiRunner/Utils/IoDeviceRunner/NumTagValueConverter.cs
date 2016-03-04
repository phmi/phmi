using System;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiRunner.Utils.IoDeviceRunner
{
    public class NumTagValueConverter
    {
        private readonly double _add;
        private readonly double _div;
        private readonly double _mul;
        private readonly bool _needCalculation;
        private readonly Type _type;

        public NumTagValueConverter(NumTag numericTag)
        {
            _type = numericTag.TagType;
            if (numericTag.RawMinDb == null || numericTag.RawMaxDb == null || numericTag.EngMinDb == null ||
                numericTag.EngMaxDb == null || numericTag.RawMaxDb - numericTag.RawMinDb == 0)
                return;
            _needCalculation = true;
            _mul = (double) numericTag.EngMaxDb - (double) numericTag.EngMinDb;
            _add = (double) numericTag.EngMinDb*(double) numericTag.RawMaxDb -
                   (double) numericTag.EngMaxDb*(double) numericTag.RawMinDb;
            _div = (double) numericTag.RawMaxDb - (double) numericTag.RawMinDb;
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
