namespace PHmiClient.Utils.Notifications
{
    public class NotificationReporterFactory : INotificationReporterFactory
    {
        public INotificationReporter Create(ITimeService timeService)
        {
            return new NotificationReporter(timeService, new TimerService());
        }
    }
}
