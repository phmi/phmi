using System;
using System.Runtime.Serialization;

namespace PHmiClient.Wcf.ServiceTypes
{
    [DataContract]
    internal class RemapTrendsResult
    {
        [DataMember]
        public WcfNotification[] Notifications { get; set; }

        [DataMember]
        public Tuple<DateTime, double?[]>[][] Pages { get; set; }

        [DataMember]
        public Tuple<DateTime, double?[]>[][] Samples { get; set; }
    }
}
