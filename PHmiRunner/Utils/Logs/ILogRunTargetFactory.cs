using PHmiClient.Utils;
using PHmiModel;

namespace PHmiRunner.Utils.Logs
{
    public interface ILogRunTargetFactory
    {
        ILogMaintainer Create(string connectionString, PHmiModel.Entities.Log log, ITimeService timeService);
    }
}
