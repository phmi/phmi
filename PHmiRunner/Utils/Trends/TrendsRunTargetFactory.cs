using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiModel;
using PHmiTools.Utils.Npg;
using System;
using System.Linq;
using PHmiModel.Entities;

namespace PHmiRunner.Utils.Trends
{
    public class TrendsRunTargetFactory : ITrendsRunTargetFactory
    {
        public ITrendsRunTarget Create(string connectionString, IProject project, TrendCategory trendCategory, ITimeService timeService)
        {
            var npgsqlConnectionFactory = new NpgsqlConnectionFactory(connectionString);
            var repositoryFactory = new TrendsRepositoryFactory(
                npgsqlConnectionFactory,
                trendCategory.Id,
                trendCategory.TrendTags.Select(t => t.Id).ToArray());
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
