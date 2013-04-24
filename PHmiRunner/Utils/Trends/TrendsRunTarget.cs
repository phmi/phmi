using System;
using System.Collections.Generic;
using System.Linq;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Pagination;
using PHmiModel;
using PHmiResources.Loc;

namespace PHmiRunner.Utils.Trends
{
    public class TrendsRunTarget : ITrendsRunTarget
    {
        private class TrendTagInfo
        {
            public TrendTagInfo(int id, Func<bool> triggerValue, Func<double?> value)
            {
                Id = id;
                TriggerValue = triggerValue;
                Value = value;
            }

            public int Id { get; private set; }

            public Func<bool> TriggerValue { get; private set; }

            public Func<double?> Value { get; private set; } 
        }

        private readonly string _name;
        private readonly TimeSpan? _timeToStore;
        private readonly INotificationReporter _reporter;
        private readonly ITrendsRepositoryFactory _repositoryFactory;
        private readonly ITimeService _timeService;
        private readonly ITrendTableSelector _tableSelector;
        private readonly IDictionary<int, TrendTagInfo> _trendsInfo = new Dictionary<int,TrendTagInfo>();

        public TrendsRunTarget(
            trend_categories trendCategory,
            INotificationReporter reporter,
            ITrendsRepositoryFactory repositoryFactory,
            IProject project,
            ITimeService timeService,
            ITrendTableSelector tableSelector)
        {
            _name = string.Format("{0} \"{1}\"", Res.Trends, trendCategory.name);
            _timeToStore = trendCategory.time_to_store.HasValue ? new TimeSpan(trendCategory.time_to_store.Value) as TimeSpan? : null; 
            foreach (var t in trendCategory.trend_tags.ToArray())
            {
                Func<bool> triggerValueGetter;
                if (t.dig_tags == null)
                {
                    triggerValueGetter = () => true;
                }
                else
                {
                    var trIoDevId = t.dig_tags.io_devices.id;
                    var trId = t.dig_tags.id;
                    triggerValueGetter = () => project.IoDeviceRunTargets[trIoDevId].GetDigitalValue(trId) == true;
                }
                var ioDeviceId = t.num_tags.io_devices.id;
                var tagId = t.num_tags.id;
                var trendInfo = new TrendTagInfo(
                    t.id,
                    triggerValueGetter,
                    () => project.IoDeviceRunTargets[ioDeviceId].GetNumericValue(tagId));
                _trendsInfo.Add(t.id, trendInfo);
            }
            _reporter = reporter;
            _repositoryFactory = repositoryFactory;
            _timeService = timeService;
            _tableSelector = tableSelector;
        }

        public void Run()
        {
            using (var repository = _repositoryFactory.Create())
            {
                DeleteOldTrendSamples(repository);
                ProcessTrends(repository);
            }
        }

        private void DeleteOldTrendSamples(ITrendsRepository repository)
        {
            if (!_timeToStore.HasValue)
                return;
            repository.DeleteOld(_timeService.UtcTime - _timeToStore.Value);
        }

        private void ProcessTrends(ITrendsRepository repository)
        {
            var time = _timeService.UtcTime;
            var infoToInsert = (from info in _trendsInfo.Values
                                where info.TriggerValue()
                                let value = info.Value()
                                where value.HasValue
                                select new Tuple<int, double>(info.Id, value.Value)).ToArray();
            if (infoToInsert.Any())
            {
                repository.Insert(time, _tableSelector.NextTable(), infoToInsert);
            }
        }

        public void Clean()
        {
        }

        public string Name
        {
            get { return _name; }
        }

        public INotificationReporter Reporter
        {
            get { return _reporter; }
        }

        public Tuple<DateTime, double?[]>[] GetPage(int[] trendTagIds, CriteriaType criteriaType, DateTime criteria, int maxCount)
        {
            using (var repository = _repositoryFactory.Create())
            {
                return repository.GetPage(trendTagIds, criteriaType, criteria, maxCount);
            }
        }

        public Tuple<DateTime, double?[]>[] GetSamples(int[] trendTagIds, DateTime startTime, DateTime? endTime, int rarerer)
        {
            using (var repository = _repositoryFactory.Create())
            {
                return repository.GetSamples(trendTagIds, startTime, endTime, rarerer);
            }
        }
    }
}
