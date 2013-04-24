using System.Runtime.Serialization;

namespace PHmiClient.Utils.Pagination
{
    [DataContract]
    public enum CriteriaType
    {
        [EnumMember]
        DownFromInfinity,

        [EnumMember]
        DownFromOrEqual,

        [EnumMember]
        DownFrom,

        [EnumMember]
        UpFrom,

        [EnumMember]
        UpFromOrEqual,

        [EnumMember]
        UpFromInfinity
    }
}
