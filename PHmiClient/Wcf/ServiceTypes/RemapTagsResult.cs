using System.Runtime.Serialization;

namespace PHmiClient.Wcf.ServiceTypes
{
    [DataContract]
    internal sealed class RemapTagsResult
    {
        [DataMember]
        public WcfNotification[] Notifications { get; set; }

        [DataMember]
        public bool?[] DigReadValues { get; set; }

        [DataMember]
        public double?[] NumReadValues { get; set; }
    }
}
