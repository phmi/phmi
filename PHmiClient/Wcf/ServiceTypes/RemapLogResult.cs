using PHmiClient.Logs;
using System;
using System.Runtime.Serialization;

namespace PHmiClient.Wcf.ServiceTypes
{
    [DataContract]
    public sealed class RemapLogResult
    {
        [DataMember]
        public DateTime SaveResult { get; set; }

        [DataMember]
        public LogItem[][] Items { get; set; }
    }
}
