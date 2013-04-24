using System;

namespace PHmiClient.Trends
{
    public abstract class TrendTagAbstract : ITrendTag
    {
        public abstract TrendsCategoryAbstract Category { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        public abstract string Format { get; }

        internal abstract int Id { get; }

        public abstract string EngUnit { get; }

        public abstract void GetSamples(
            DateTime startTime, DateTime? endTime, int rarerer, Action<Tuple<DateTime, double>[]> callback);

        public abstract double? MinValue { get; }

        public abstract double? MaxValue { get; }
    }
}
