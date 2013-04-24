using System;
using System.Runtime.Serialization;

namespace PHmiClient.Alarms
{
    [DataContract]
    public sealed class AlarmSampleId
    {
        public AlarmSampleId(DateTime startTime, int alarmId)
        {
            StartTime = startTime;
            AlarmId = alarmId;
        }

        public AlarmSampleId(Alarm alarm) : this(alarm.StartTime, alarm.AlarmId) { }

        [DataMember]
        public DateTime StartTime { get; private set; }

        [DataMember]
        public int AlarmId { get; private set; }

        public override bool Equals(object obj)
        {
            var a = obj as AlarmSampleId;
            if (a == null)
                return false;
            return StartTime.Equals(a.StartTime) && AlarmId.Equals(a.AlarmId);
        }

        public override int GetHashCode()
        {
            return StartTime.GetHashCode() ^ AlarmId.GetHashCode();
        }
    }
}
