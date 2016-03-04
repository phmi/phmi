using PHmiClient.Utils;
using PHmiModel;
using PHmiTools.Utils.Npg;

namespace PHmiRunner.Utils.Logs
{
    public class LogMaintainerFactory : ILogRunTargetFactory
    {
        public ILogMaintainer Create(string connectionString, PHmiModel.Entities.Log log, ITimeService timeService)
        {
            var npgsqlConnectionFactory = new NpgsqlConnectionFactory(connectionString);
            var logRepository = new LogRepository(log.Id);
            using (var connection = npgsqlConnectionFactory.Create())
            {
                logRepository.EnsureTable(connection);
            }
            return new LogMaintainer(
                log,
                logRepository,
                timeService,
                npgsqlConnectionFactory);
        }
    }
}
