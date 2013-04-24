using PHmiClient.Alarms;
using PHmiClient.Users;
using PHmiClient.Utils.Pagination;
using System;
using System.Runtime.Serialization;

namespace PHmiClient.Wcf.ServiceTypes
{
    [DataContract]
    internal sealed class RemapAlarmsParameter
    {
        [DataMember]
        public int CategoryId { get; set; }

        [DataMember]
        public Tuple<AlarmSampleId[], Identity>[] AcknowledgeParameters { get; set; }

        [DataMember]
        public Tuple<CriteriaType, AlarmSampleId, int>[] CurrentParameters { get; set; }

        [DataMember]
        public Tuple<CriteriaType, AlarmSampleId, int>[] HistoryParameters { get; set; }

        [DataMember]
        public bool GetStatus { get; set; }
    }
}
