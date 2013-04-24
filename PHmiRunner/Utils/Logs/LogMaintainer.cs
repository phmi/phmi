using Npgsql;
using PHmiClient.Logs;
using PHmiClient.Utils;
using PHmiClient.Utils.Pagination;
using PHmiModel;
using PHmiTools.Utils.Npg;
using System;
using System.Linq;

namespace PHmiRunner.Utils.Logs
{
    public class LogMaintainer : ILogMaintainer
    {
        private readonly ILogRepository _repository;
        private readonly ITimeService _timeService;
        private readonly INpgsqlConnectionFactory _connectionFactory;
        private readonly TimeSpan? _timeToStore;
        private readonly DateTime _defaultTime = new DateTime();

        public LogMaintainer(
            logs log,
            ILogRepository repository,
            ITimeService timeService,
            INpgsqlConnectionFactory connectionFactory)
        {
            _repository = repository;
            _timeService = timeService;
            _connectionFactory = connectionFactory;
            _timeToStore = log.time_to_store.HasValue ? new TimeSpan(log.time_to_store.Value) as TimeSpan? : null;
        }
        
        public DateTime Save(LogItem item)
        {
            using (var connection = _connectionFactory.Create())
            {
                DeleteOld(connection);
                if (item.Time == _defaultTime)
                {
                    item.Time = _timeService.UtcTime;
                    _repository.Insert(connection, item);
                }
                else if (!_repository.Update(connection, item))
                {
                    _repository.Insert(connection, item);
                }
                return item.Time;
            }
        }

        private void DeleteOld(NpgsqlConnection connection)
        {
            if (!_timeToStore.HasValue)
                return;
            var oldTime = _timeService.UtcTime - _timeToStore.Value;
            _repository.DeleteOld(connection, oldTime);
        }

        public void Delete(DateTime[] times)
        {
            using (var connection = _connectionFactory.Create())
            {
                foreach (var time in times)
                {
                    _repository.Delete(connection, time);
                }
            }
        }

        public LogItem[][] GetItems(Tuple<CriteriaType, DateTime, int, bool>[] parameters)
        {
            using (var connection = _connectionFactory.Create())
            {
                return parameters.Select(p => _repository.GetItems(connection, p.Item1, p.Item2, p.Item3, p.Item4)).ToArray();
            }
        }
    }
}
