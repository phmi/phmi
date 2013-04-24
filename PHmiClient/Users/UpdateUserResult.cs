using System.Runtime.Serialization;

namespace PHmiClient.Users
{
    [DataContract]
    public enum UpdateUserResult
    {
        [EnumMember]
        Success,
        [EnumMember]
        NameConflict,
        [EnumMember]
        UserNotFound,
        [EnumMember]
        NullValue,
        [EnumMember]
        Fail
    }
}
