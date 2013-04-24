using System.Runtime.Serialization;

namespace PHmiClient.Users
{
    [DataContract]
    public enum InsertUserResult
    {
        [EnumMember]
        Success,
        [EnumMember]
        IdConflict,
        [EnumMember]
        NameConflict,
        [EnumMember]
        NullValue,
        [EnumMember]
        Fail
    }
}
