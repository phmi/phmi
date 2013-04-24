using System;
using PHmiClient.Utils.Pagination;

namespace PHmiRunner.Utils.Trends
{
    public interface ITrendsRepository : IDisposable
    {
        void EnsureTables();
        void DeleteOld(DateTime oldTime);
        void Insert(DateTime time, int tableIndex, Tuple<int, double>[] samples);
        Tuple<DateTime, double?[]>[] GetPage(int[] trendTagIds, CriteriaType criteriaType, DateTime criteria, int maxCount);
        Tuple<DateTime, double?[]>[] GetSamples(int[] trendTagIds, DateTime startTime, DateTime? endTime, int rarerer);
    }
}
