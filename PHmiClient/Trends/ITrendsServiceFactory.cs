using PHmiClient.Utils.Notifications;

namespace PHmiClient.Trends
{
    internal interface ITrendsServiceFactory
    {
        ITrendsService Create(IReporter reporter);
    }
}
