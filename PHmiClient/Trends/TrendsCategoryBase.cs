using System;
using System.Collections.Generic;
using System.Linq;
using PHmiClient.Utils.Pagination;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClient.Trends
{
    public class TrendsCategoryBase : TrendsCategoryAbstract
    {
        private readonly int _id;
        private readonly string _name;
        private readonly TimeSpan _period;
        private readonly IList<Tuple<int, Tuple<DateTime, DateTime?, int>, Action<Tuple<DateTime, double>[]>>> _samplesQueries
            = new List<Tuple<int, Tuple<DateTime, DateTime?, int>, Action<Tuple<DateTime, double>[]>>>();
        private readonly IList<Tuple<int, Tuple<CriteriaType, DateTime, int>, Action<Tuple<DateTime, double>[]>>> _pageQueries
            = new List<Tuple<int, Tuple<CriteriaType, DateTime, int>, Action<Tuple<DateTime, double>[]>>>();

        private Action<Tuple<DateTime, double>[]>[][] _currentSamplesCallabacks;
        private Action<Tuple<DateTime, double>[]>[][] _pageSamplesCallbacks;

        protected internal TrendsCategoryBase(int id, string name, long periodTicks)
        {
            _id = id;
            _name = name;
            _period = new TimeSpan(periodTicks);
        }

        internal override int Id
        {
            get { return _id; }
        }

        public override string Name
        {
            get { return _name; }
        }

        public override TimeSpan Period
        {
            get { return _period; }
        }

        protected override ITrendTag AddTrendTag(
            int id,
            string name,
            Func<string> descriptionGetter,
            Func<string> formatGetter,
            Func<string> engUnitGetter,
            double? minValue,
            double? maxValue)
        {
            var tag = new TrendTag(this, id, name, descriptionGetter, formatGetter, engUnitGetter, minValue, maxValue);
            return tag;
        }

        internal override RemapTrendsParameter CreateRemapParameter()
        {
            Tuple<Tuple<DateTime, DateTime?, int>, Tuple<int, Action<Tuple<DateTime, double>[]>>[]>[] currentSamplesQueries;
            lock (_samplesQueries)
            {
                currentSamplesQueries = _samplesQueries
                    .GroupBy(s => s.Item2)
                    .Select(g => new Tuple<Tuple<DateTime, DateTime?, int>, Tuple<int, Action<Tuple<DateTime, double>[]>>[]>(
                            g.Key,
                            g.Select(s => new Tuple<int, Action<Tuple<DateTime, double>[]>>(s.Item1, s.Item3)).ToArray()))
                    .ToArray();
                _samplesQueries.Clear();
            }
            Tuple<Tuple<CriteriaType, DateTime, int>, Tuple<int, Action<Tuple<DateTime, double>[]>>[]>[] pageSamplesQueries;
            lock (_pageQueries)
            {
                pageSamplesQueries = _pageQueries
                    .GroupBy(p => p.Item2)
                    .Select(g => new Tuple<Tuple<CriteriaType, DateTime, int>, Tuple<int, Action<Tuple<DateTime, double>[]>>[]>(
                            g.Key,
                            g.Select(s => new Tuple<int, Action<Tuple<DateTime, double>[]>>(s.Item1, s.Item3)).ToArray()))
                    .ToArray();
                _pageQueries.Clear();
            }
            if (!currentSamplesQueries.Any() && !pageSamplesQueries.Any())
            {
                return null;
            }
            var samplesParameters = currentSamplesQueries
                .Select(s => new Tuple<int[], DateTime, DateTime?, int>(
                    s.Item2.Select(i => i.Item1).ToArray(), s.Item1.Item1, s.Item1.Item2, s.Item1.Item3)).ToArray();
            _currentSamplesCallabacks = currentSamplesQueries.Select(s => s.Item2.Select(t => t.Item2).ToArray()).ToArray();
            var pageParameters = pageSamplesQueries
                .Select(s => new Tuple<int[], CriteriaType, DateTime, int>(
                    s.Item2.Select(i => i.Item1).ToArray(), s.Item1.Item1, s.Item1.Item2, s.Item1.Item3)).ToArray();
            _pageSamplesCallbacks = pageSamplesQueries.Select(s => s.Item2.Select(t => t.Item2).ToArray()).ToArray();

            var parameter = new RemapTrendsParameter
                {
                    CategoryId = _id,
                    SamplesParameters = samplesParameters,
                    PageParameters = pageParameters
                };
            return parameter;
        }

        internal override void ApplyRemapResult(RemapTrendsResult result)
        {
            if (result == null)
            {
                var empty = new Tuple<DateTime, double>[0];
                if (_currentSamplesCallabacks != null)
                {
                    foreach (var action in _currentSamplesCallabacks.SelectMany(callaback => callaback))
                    {
                        action(empty);
                    }
                }
                if (_pageSamplesCallbacks != null)
                {
                    foreach (var action in _pageSamplesCallbacks.SelectMany(callback => callback))
                    {
                        action(empty);
                    }
                }
                return;
            }
            for (var i = 0; i < result.Samples.Length; i++)
            {
                var s = result.Samples[i];
                var callbacks = _currentSamplesCallabacks[i];
                for (var j = 0; j < callbacks.Length; j++)
                {
                    var samples = (from item in s
                                   let v = item.Item2[j]
                                   where v.HasValue
                                   select new Tuple<DateTime, double>(item.Item1, v.Value)).ToArray();
                    callbacks[j](samples);
                }
            }
            for (var i = 0; i < result.Pages.Length; i++)
            {
                var s = result.Pages[i];
                var callbacks = _pageSamplesCallbacks[i];
                for (var j = 0; j < callbacks.Length; j++)
                {
                    var samples = (from item in s
                                   let v = item.Item2[j]
                                   where v.HasValue
                                   select new Tuple<DateTime, double>(item.Item1, v.Value)).ToArray();
                    callbacks[j](samples);
                }
            }
        }

        internal override void GetSamples(
            int trendTagId, DateTime startTime, DateTime? endTime, int rarerer, Action<Tuple<DateTime, double>[]> callback)
        {
            lock (_samplesQueries)
            {
                _samplesQueries.Add(new Tuple<int, Tuple<DateTime, DateTime?, int>, Action<Tuple<DateTime, double>[]>>(
                    trendTagId, new Tuple<DateTime, DateTime?, int>(startTime, endTime, rarerer), callback));
            }
        }

        internal override void GetPage(
            int trendTagId, CriteriaType criteriaType, DateTime criteria, int maxCount, Action<Tuple<DateTime, double>[]> callback)
        {
            lock (_pageQueries)
            {
                _pageQueries.Add(new Tuple<int, Tuple<CriteriaType, DateTime, int>, Action<Tuple<DateTime, double>[]>>(
                    trendTagId, new Tuple<CriteriaType, DateTime, int>(criteriaType, criteria, maxCount), callback));
            }
        }
    }
}
