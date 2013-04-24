using PHmiClient.Alarms;
using System.Runtime.Serialization;

namespace PHmiClient.Wcf.ServiceTypes
{
    [DataContract]
    internal sealed class RemapAlarmResult
    {
        [DataMember]
        public WcfNotification[] Notifications { get; set; }

        [DataMember]
        public Alarm[][] Current { get; set; }

        [DataMember]
        public Alarm[][] History { get; set; }

        [DataMember]
        public bool HasUnacknowledged { get; set; }

        [DataMember]
        public bool HasActive { get; set; }
    }
}
