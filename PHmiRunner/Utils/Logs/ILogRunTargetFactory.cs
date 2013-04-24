using PHmiClient.Utils;
using PHmiModel;

namespace PHmiRunner.Utils.Logs
{
    public interface ILogRunTargetFactory
    {
        ILogMaintainer Create(string connectionString, logs log, ITimeService timeService);
    }
}
