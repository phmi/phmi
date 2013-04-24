using System.Linq;
using System.ServiceProcess;

namespace PHmiService.Utils
{
    public static class ServiceHelper
    {
        public static string[] GetServiceNames(string service)
        {
            var services = ServiceController.GetServices();
            return services.Where(s => s.ServiceName.Contains(service)).Select(s => s.ServiceName).ToArray();
        }
    }
}
