using PHmiClient.Utils.Notifications;

namespace PHmiClient.Alarms
{
    internal class AlarmServiceFactory : IAlarmServiceFactory
    {
        public IAlarmService Create(IReporter reporter)
        {
            return new AlarmService(reporter);
        }
    }
}
