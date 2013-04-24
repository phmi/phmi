using PHmiClient.Utils;
using PHmiModel;

namespace PHmiRunner.Utils.Alarms
{
    public interface IAlarmsRunTargetFactory
    {
        IAlarmsRunTarget Create(string connectionString, IProject project, alarm_categories alarmCategory, ITimeService timeService);
    }
}
