using System;
using System.Runtime.Serialization;

namespace PHmiClient.Wcf.ServiceTypes
{
    [DataContract]
    internal sealed class WcfNotification
    {
        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string ShortDescription { get; set; }

        [DataMember]
        public string LongDescription { get; set; }
    }
}
