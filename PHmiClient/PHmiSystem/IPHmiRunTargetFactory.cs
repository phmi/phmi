using PHmiClient.Utils.Notifications;
using PHmiClient.Wcf;

namespace PHmiClient.PHmiSystem
{
    internal interface IPHmiRunTargetFactory
    {
        IPHmiRunTarget Create(
            INotificationReporter reporter, 
            IServiceClientFactory clientFactory,
            params IServiceRunTarget[] targets);
    }
}
