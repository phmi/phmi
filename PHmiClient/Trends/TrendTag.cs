using System;

namespace PHmiClient.Trends
{
    public class TrendTag : TrendTagAbstract
    {
        private readonly TrendsCategoryAbstract _category;
        private readonly int _id;
        private readonly string _name;
        private readonly Func<string> _descriptionGetter;
        private readonly Func<string> _formatGetter; 
        private readonly Func<string> _engUnitGetter;
        private readonly double? _minValue;
        private readonly double? _maxValue;

        internal TrendTag(
            TrendsCategoryAbstract category,
            int id,
            string name,
            Func<string> descriptionGetter,
            Func<string> formatGetter,
            Func<string> engUnitGetter,
            double? minValue,
            double? maxValue)
        {
            _category = category;
            _id = id;
            _name = name;
            _descriptionGetter = descriptionGetter;
            _formatGetter = formatGetter;
            _engUnitGetter = engUnitGetter;
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public override TrendsCategoryAbstract Category
        {
            get { return _category; }
        }

        public override string Format
        {
            get { return _formatGetter(); }
        }

        internal override int Id
        {
            get { return _id; }
        }

        public override string Name
        {
            get { return _name; }
        }

        public override string Description
        {
            get { return _descriptionGetter(); }
        }
        
        public override string EngUnit
        {
            get { return _engUnitGetter(); }
        }

        public override void GetSamples(DateTime startTime, DateTime? endTime, int rarerer, Action<Tuple<DateTime, double>[]> callback)
        {
            _category.GetSamples(_id, startTime, endTime, rarerer, callback);
        }

        public override double? MinValue
        {
            get { return _minValue; }
        }

        public override double? MaxValue
        {
            get { return _maxValue; }
        }
    }
}
