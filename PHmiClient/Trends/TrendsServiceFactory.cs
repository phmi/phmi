using PHmiClient.Utils.Notifications;

namespace PHmiClient.Trends
{
    internal class TrendsServiceFactory : ITrendsServiceFactory
    {
        public ITrendsService Create(IReporter reporter)
        {
            return new TrendsService(reporter);
        }
    }
}
