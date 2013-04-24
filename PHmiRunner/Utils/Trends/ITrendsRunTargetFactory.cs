using PHmiClient.Utils;
using PHmiModel;

namespace PHmiRunner.Utils.Trends
{
    public interface ITrendsRunTargetFactory
    {
        ITrendsRunTarget Create(string connectionString, IProject project, trend_categories alarmCategory, ITimeService timeService);
    }
}
