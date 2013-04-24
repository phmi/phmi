using System;
using PHmiClient.Utils.Pagination;
using PHmiClient.Utils.Runner;

namespace PHmiRunner.Utils.Trends
{
    public interface ITrendsRunTarget : IRunTarget
    {
        Tuple<DateTime, double?[]>[] GetPage(int[] trendTagIds, CriteriaType criteriaType, DateTime criteria, int maxCount);

        Tuple<DateTime, double?[]>[] GetSamples(int[] trendTagIds, DateTime startTime, DateTime? endTime, int rarerer);
    }
}
