using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiModel;
using PHmiTools.Utils.Npg;
using System;
using System.Linq;

namespace PHmiRunner.Utils.Trends
{
    public class TrendsRunTargetFactory : ITrendsRunTargetFactory
    {
        public ITrendsRunTarget Create(string connectionString, IProject project, trend_categories trendCategory, ITimeService timeService)
        {
            var npgsqlConnectionFactory = new NpgsqlConnectionFactory(connectionString);
            var repositoryFactory = new TrendsRepositoryFactory(
                npgsqlConnectionFactory,
                trendCategory.id,
                trendCategory.trend_tags.Select(t => t.id).ToArray());
            using (var repository = repositoryFactory.Create())
            {
                repository.EnsureTables();
            }

            return new TrendsRunTarget(
                trendCategory,
                new NotificationReporter(timeService) { LifeTime = TimeSpan.FromTicks(0) },
                repositoryFactory,
                project,
                timeService,
                new TrendTableSelector());
        }
    }
}
