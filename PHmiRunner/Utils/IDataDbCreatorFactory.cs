using PHmiClient.Utils.Notifications;

namespace PHmiRunner.Utils
{
    public interface IDataDbCreatorFactory
    {
        IDataDbCreator Create(string connectionString, IReporter reporter);
    }
}
