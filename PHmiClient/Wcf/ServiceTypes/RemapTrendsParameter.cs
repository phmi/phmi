using PHmiClient.Utils.Pagination;
using System;
using System.Runtime.Serialization;

namespace PHmiClient.Wcf.ServiceTypes
{
    [DataContract]
    internal class RemapTrendsParameter
    {
        [DataMember]
        public int CategoryId { get; set; }

        [DataMember]
        public Tuple<int[], CriteriaType, DateTime, int>[] PageParameters { get; set; }

        [DataMember]
        public Tuple<int[], DateTime, DateTime?, int>[] SamplesParameters { get; set; }
    }
}
