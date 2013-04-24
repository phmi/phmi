using PHmiClient.Utils;
using PHmiClient.Utils.Runner;
using PHmiClient.Wcf;

namespace PHmiRunner.Utils.Wcf
{
    public class ServiceRunnerFactory : IServiceRunnerFactory
    {
        public IRunner Create(IProject project, string server, string guid, ITimeService timeService)
        {
            return new ServiceRunner(new Service(project, guid, timeService), server, new DefaultBindingFactory(), new ServerUriFactory());
        }
    }
}
