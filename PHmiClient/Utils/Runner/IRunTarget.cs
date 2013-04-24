using PHmiClient.Utils.Notifications;

namespace PHmiClient.Utils.Runner
{
    public interface IRunTarget
    {
        void Run();
        void Clean();
        string Name { get; }
        INotificationReporter Reporter { get; }
    }
}
