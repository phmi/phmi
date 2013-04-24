using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiModel;
using PHmiTools.Utils.Npg;
using System;

namespace PHmiRunner.Utils.Alarms
{
    public class AlarmsRunTargetFactory : IAlarmsRunTargetFactory
    {
        public IAlarmsRunTarget Create(string connectionString, IProject project, alarm_categories alarmCategory, ITimeService timeService)
        {
            var npgsqlConnectionFactory = new NpgsqlConnectionFactory(connectionString);
            var alarmsRepository = new AlarmsRepository(alarmCategory.id);
            using (var connection = npgsqlConnectionFactory.Create())
            {
                alarmsRepository.EnsureTable(connection);
            }
            return new AlarmsRunTarget(
                alarmCategory,
                new NotificationReporter(timeService) {LifeTime = TimeSpan.FromTicks(0)},
                alarmsRepository,
                project,
                timeService,
                npgsqlConnectionFactory);
        }
    }
}
