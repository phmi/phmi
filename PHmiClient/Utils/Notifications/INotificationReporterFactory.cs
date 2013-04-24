namespace PHmiClient.Utils.Notifications
{
    public interface INotificationReporterFactory
    {
        INotificationReporter Create(ITimeService timeService);
    }
}
