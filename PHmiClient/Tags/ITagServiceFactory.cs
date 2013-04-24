using PHmiClient.Utils.Notifications;

namespace PHmiClient.Tags
{
    internal interface ITagServiceFactory
    {
        ITagService Create(IReporter reporter);
    }
}
