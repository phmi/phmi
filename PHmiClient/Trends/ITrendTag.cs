using System;

namespace PHmiClient.Trends
{
    public interface ITrendTag
    {
        TrendsCategoryAbstract Category { get; }
        string Name { get; }
        string Description { get; }
        string Format { get; }
        string EngUnit { get; }
        void GetSamples(DateTime startTime, DateTime? endTime, int rarerer, Action<Tuple<DateTime, double>[]> callback);
        double? MinValue { get; }
        double? MaxValue { get; }
    }
}
