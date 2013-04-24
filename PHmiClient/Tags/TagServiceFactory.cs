using PHmiClient.Utils.Notifications;

namespace PHmiClient.Tags
{
    internal class TagServiceFactory : ITagServiceFactory
    {
        public ITagService Create(IReporter reporter)
        {
            return new TagService(reporter);
        }
    }
}
