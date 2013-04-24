using System;
using PHmiClient.Utils;

namespace PHmiClient.Tags
{
    public class NumericTag : Tag<double?>, INumericTag
    {
        private readonly Func<string> _formatGetter;
        private readonly Func<string> _engUnitGetter;
        private readonly double? _minValue;
        private readonly double? _maxValue;
        
        internal NumericTag(IDispatcherService dispatcherService, int id, string name, Func<string> descriptionGetter,
            Func<string> formatGetter, Func<string> engUnitGetter, double? minValue, double? maxValue)
            : base(dispatcherService, id, name, descriptionGetter)
        {
            _formatGetter = formatGetter;
            _engUnitGetter = engUnitGetter;
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public double? MinValue
        {
            get { return _minValue; }
        }

        public double? MaxValue
        {
            get { return _maxValue; }
        }

        public string ValueString
        {
            get
            {
                var value = Value;
                return value.HasValue ? value.Value.ToString(_formatGetter.Invoke()) + _engUnitGetter.Invoke() : null;
            }
        }

        protected override void OnPropertyChanged(string property)
        {
            base.OnPropertyChanged(property);
            if (property == "Value")
            {
                OnPropertyChanged("ValueString");
            }
        }

        public override double? Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                if (value > _maxValue)
                    base.Value = _maxValue;
                else if (value < _minValue)
                    base.Value = _minValue;
                else
                    base.Value = value;
            }
        }
    }
}
