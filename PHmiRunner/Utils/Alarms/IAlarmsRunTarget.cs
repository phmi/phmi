using System;
using PHmiClient.Alarms;
using PHmiClient.Users;
using PHmiClient.Utils.Pagination;
using PHmiClient.Utils.Runner;

namespace PHmiRunner.Utils.Alarms
{
    public interface IAlarmsRunTarget : IRunTarget
    {
        Alarm[] GetCurrentAlarms(CriteriaType criteriaType, AlarmSampleId criteria, int maxCount);
        Alarm[] GetHistoryAlarms(CriteriaType criteriaType, AlarmSampleId criteria, int maxCount);
        Tuple<bool, bool> GetHasActiveAndUnacknowledged();
        void Acknowledge(AlarmSampleId[] alarms, Identity identity);
    }
}
