using PHmiClient.PHmiSystem;

namespace PHmiClient.Alarms
{
    internal interface IAlarmService : IServiceRunTarget
    {
        void Add(AlarmCategoryAbstract category);
    }
}
