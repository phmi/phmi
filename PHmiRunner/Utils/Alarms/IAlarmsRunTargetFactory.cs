using PHmiClient.Utils;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiRunner.Utils.Alarms
{
    public interface IAlarmsRunTargetFactory
    {
        IAlarmsRunTarget Create(string connectionString, IProject project, AlarmCategory alarmCategory, ITimeService timeService);
    }
}
