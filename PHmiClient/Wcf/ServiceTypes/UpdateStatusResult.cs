using System;
using System.Runtime.Serialization;

namespace PHmiClient.Wcf.ServiceTypes
{
    [DataContract]
    internal sealed class UpdateStatusResult
    {
        [DataMember]
        public DateTime Time { get; set; }
    }
}
