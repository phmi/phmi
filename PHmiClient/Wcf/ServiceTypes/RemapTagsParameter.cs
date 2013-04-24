using System.Runtime.Serialization;

namespace PHmiClient.Wcf.ServiceTypes
{
    [DataContract]
    internal sealed class RemapTagsParameter
    {
        [DataMember]
        public int IoDeviceId { get; set; }

        [DataMember]
        public int[] DigWriteIds { get; set; }

        [DataMember]
        public bool[] DigWriteValues { get; set; }

        [DataMember]
        public int[] NumWriteIds { get; set; }

        [DataMember]
        public double[] NumWriteValues { get; set; }

        [DataMember]
        public int[] DigReadIds { get; set; }

        [DataMember]
        public int[] NumReadIds { get; set; }
    }
}
