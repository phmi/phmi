using PHmiClient.Utils;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiRunner.Utils.Trends
{
    public interface ITrendsRunTargetFactory
    {
        ITrendsRunTarget Create(string connectionString, IProject project, TrendCategory alarmCategory, ITimeService timeService);
    }
}
