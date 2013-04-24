using PHmiClient.Utils;
using PHmiClient.Utils.Runner;

namespace PHmiRunner.Utils.Wcf
{
    public interface IServiceRunnerFactory
    {
        IRunner Create(IProject project, string server, string guid, ITimeService timeService);
    }
}
