using System;
using PHmiClient.Utils.Pagination;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClient.Trends
{
    public abstract class TrendsCategoryAbstract
    {
        internal abstract int Id { get; }

        public abstract string Name { get; }

        public abstract TimeSpan Period { get; }

        protected abstract ITrendTag AddTrendTag(int id, string name, Func<string> descriptionGetter,
            Func<string> formatGetter, Func<string> engUnitGetter, double? minValue, double? maxValue);

        internal abstract RemapTrendsParameter CreateRemapParameter();

        internal abstract void ApplyRemapResult(RemapTrendsResult result);

        internal abstract void GetSamples(
            int trendTagId,
            DateTime startTime,
            DateTime? endTime,
            int rarerer,
            Action<Tuple<DateTime, double>[]> callback);

        internal abstract void GetPage(
            int trendTagId,
            CriteriaType criteriaType,
            DateTime criteria,
            int maxCount,
            Action<Tuple<DateTime, double>[]> callback);
    }
}
