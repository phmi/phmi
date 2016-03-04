using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using PHmiClient.Alarms;
using PHmiClient.Users;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Pagination;
using PHmiModel;
using PHmiModel.Entities;
using PHmiResources.Loc;
using PHmiTools.Utils.Npg;

namespace PHmiRunner.Utils.Alarms
{
    public class AlarmsRunTarget : IAlarmsRunTarget
    {
        private class AlarmStatus
        {
            public AlarmStatus(bool acknowledgeable)
            {
                Acknowledgeable = acknowledgeable;
            }

            public bool Acknowledgeable { get; private set; }

            public bool? Value { get; set; } 
        }

        private readonly INotificationReporter _reporter;
        private readonly IProject _project;
        private readonly string _name;
        private readonly ITimeService _timeService;
        private readonly IAlarmsRepository _repository;
        private readonly INpgsqlConnectionFactory _connectionFactory;
        private readonly IDictionary<int, AlarmStatus> _alarmDigitalValues;
        private readonly IDictionary<int, int?> _alarmPrivileges = new Dictionary<int, int?>();
        private readonly Action _updateAlarmTagsDigitalValues;
        private readonly TimeSpan? _timeToStore;
        private NpgsqlConnection _connection;

        public AlarmsRunTarget(
            AlarmCategory alarmCategory,
            INotificationReporter reporter,
            IAlarmsRepository repository,
            IProject project,
            ITimeService timeService,
            INpgsqlConnectionFactory connectionFactory) 
        {
            _name = string.Format("{0} \"{1}\"", Res.Alarms, alarmCategory.Name);
            _reporter = reporter;
            _timeService = timeService;
            _repository = repository;
            _project = project;
            _connectionFactory = connectionFactory;
            if (alarmCategory.TimeToStoreDb.HasValue)
            {
                _timeToStore = new TimeSpan(alarmCategory.TimeToStoreDb.Value);
            }

            _alarmDigitalValues = new Dictionary<int, AlarmStatus>(alarmCategory.AlarmTags.Count);
            foreach (var t in alarmCategory.AlarmTags)
            {
                _alarmDigitalValues.Add(t.Id, new AlarmStatus(t.Acknowledgeable));
                _alarmPrivileges.Add(t.Id, t.Privilege);
            }

            _updateAlarmTagsDigitalValues = () => UpdateAlarmDigitalValues(GetIoDeviceGroups(alarmCategory));
        }

        private static IEnumerable<Tuple<int, Tuple<int, int>[]>> GetIoDeviceGroups(AlarmCategory alarmCategory)
        {
            var ioDeviceGroups = alarmCategory.AlarmTags
                .GroupBy(a => a.DigTag.IoDevice)
                .Select(g => new Tuple<int, Tuple<int, int>[]>(
                    g.Key.Id,
                    g.Select(a => new Tuple<int, int>(a.DigTag.Id, a.Id))
                    .ToArray()))
                .ToArray();
            return ioDeviceGroups;
        }

        private void UpdateAlarmDigitalValues(IEnumerable<Tuple<int, Tuple<int, int>[]>> ioDeviceGroups)
        {
            foreach (var g in ioDeviceGroups)
            {
                var ioDev = _project.IoDeviceRunTargets[g.Item1];
                ioDev.EnterReadLock();
                try
                {
                    foreach (var t in g.Item2)
                    {
                        _alarmDigitalValues[t.Item2].Value = ioDev.GetDigitalValue(t.Item1);
                    }
                }
                finally
                {
                    ioDev.ExitReadLock();
                }
            }
        }

        public string Name { get { return _name; } }

        public INotificationReporter Reporter { get { return _reporter; } }

        public void Run()
        {
            if (_connection == null)
            {
                _connection = _connectionFactory.Create();
            }
            DeleteOldAlarms();
            var time = _timeService.UtcTime;
            _updateAlarmTagsDigitalValues();
            ProcessAlarms(time);
        }

        private void DeleteOldAlarms()
        {
            if (!_timeToStore.HasValue)
                return;
            _repository.DeleteNotActive(_connection, _timeService.UtcTime - _timeToStore.Value);
        }

        private void ProcessAlarms(DateTime time)
        {
            var idsToInsert = new List<Tuple<DateTime, int, DateTime?>>();
            var idsToReset = new List<AlarmSampleId>();
            var activeIds = _repository.GetActiveIds(_connection).ToLookup(t => t.AlarmId);
            foreach (var alarmValue in _alarmDigitalValues)
            {
                if (alarmValue.Value.Value == true)
                {
                    if (activeIds.Contains(alarmValue.Key))
                    {
                        idsToReset.AddRange(activeIds[alarmValue.Key].Skip(1));
                    }
                    else
                    {
                        idsToInsert.Add(new Tuple<DateTime, int, DateTime?>(
                            time,
                            alarmValue.Key,
                            alarmValue.Value.Acknowledgeable ? null : time as DateTime?));
                    }
                }
                else
                {
                    if (activeIds.Contains(alarmValue.Key))
                    {
                        idsToReset.AddRange(activeIds[alarmValue.Key]);
                    }
                }
            }
            foreach (var ids in activeIds.Where(ids => !_alarmDigitalValues.ContainsKey(ids.Key)))
            {
                idsToReset.AddRange(ids);
            }
            if (idsToReset.Any())
            {
                _repository.Update(_connection, idsToReset.ToArray(), time);
            }
            if (idsToInsert.Any())
            {
                _repository.Insert(_connection, idsToInsert.ToArray());
            }
        }

        public void Clean()
        {
            var connection = _connection;
            _connection = null;
            connection.Dispose();
        }

        public Alarm[] GetCurrentAlarms(CriteriaType criteriaType, AlarmSampleId criteria, int maxCount)
        {
            using (var connection = _connectionFactory.Create())
            {
                return _repository.GetCurrentAlarms(connection, criteriaType, criteria, maxCount);
            }
        }

        public Alarm[] GetHistoryAlarms(CriteriaType criteriaType, AlarmSampleId criteria, int maxCount)
        {
            using (var connection = _connectionFactory.Create())
            {
                return _repository.GetHistoryAlarms(connection, criteriaType, criteria, maxCount);
            }
        }

        public Tuple<bool, bool> GetHasActiveAndUnacknowledged()
        {
            using (var connection = _connectionFactory.Create())
            {
                var hasActive = _repository.HasActiveAlarms(connection);
                var hasUnacknowledged = _repository.HasUnacknowledgedAlarms(connection);
                return new Tuple<bool, bool>(hasActive, hasUnacknowledged);
            }
        }

        public void Acknowledge(AlarmSampleId[] alarms, Identity identity)
        {
            var privilege = _project.UsersRunner.GetPrivilege(identity);
            var alarmsToAcknowledge = (from a in alarms
                                       let p = GetPrivilege(a.AlarmId)
                                       where !p.HasValue || (privilege.HasValue && (p.Value & privilege.Value) != 0)
                                       select a).ToArray();
            if (!alarmsToAcknowledge.Any())
                return;
            var userId = identity == null ? null : identity.UserId as long?;
            using (var connection = _connectionFactory.Create())
            {
                _repository.Update(connection, alarmsToAcknowledge, _timeService.UtcTime, userId);
            }
        }

        private int? GetPrivilege(int alarmId)
        {
            int? privilege;
            return _alarmPrivileges.TryGetValue(alarmId, out privilege) ? privilege : null;
        }
    }
}
