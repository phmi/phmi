using PHmiClient.Utils.Notifications;

namespace PHmiClient.Alarms
{
    internal interface IAlarmServiceFactory
    {
        IAlarmService Create(IReporter reporter);
    }
}
