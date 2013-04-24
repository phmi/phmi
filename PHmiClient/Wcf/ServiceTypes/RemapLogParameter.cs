using PHmiClient.Logs;
using PHmiClient.Utils.Pagination;
using System;
using System.Runtime.Serialization;

namespace PHmiClient.Wcf.ServiceTypes
{
    [DataContract]
    public sealed class RemapLogParameter
    {
        [DataMember]
        public int LogId { get; set; }

        [DataMember]
        public LogItem ItemToSave { get; set; }

        [DataMember]
        public DateTime[] ItemTimesToDelete { get; set; }

        [DataMember]
        public Tuple<CriteriaType, DateTime, int, bool>[] GetItemsParameters { get; set; }
    }
}
