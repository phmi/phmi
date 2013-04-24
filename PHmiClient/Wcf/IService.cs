using System.ServiceModel;
using PHmiClient.Users;
using PHmiClient.Utils.Pagination;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClient.Wcf
{
    [ServiceContract]
    internal interface IService
    {
        [OperationContract]
        UpdateStatusResult UpdateStatus();

        [OperationContract]
        RemapTagsResult[] RemapTags(RemapTagsParameter[] parameters);

        [OperationContract]
        User LogOn(string name, string password);

        [OperationContract]
        bool ChangePassword(string name, string oldPassword, string newPassword);

        [OperationContract]
        RemapAlarmResult[] RemapAlarms(RemapAlarmsParameter[] parameters);

        [OperationContract]
        RemapTrendsResult[] RemapTrends(RemapTrendsParameter[] parameters);

        [OperationContract]
        RemapLogResult[] RemapLogs(RemapLogParameter[] parameters);

        [OperationContract]
        User[] GetUsers(Identity identity, CriteriaType criteriaType, string name, int count);

        [OperationContract]
        bool SetPassword(Identity identity, long id, string password);

        [OperationContract]
        UpdateUserResult UpdateUser(Identity identity, User user);

        [OperationContract]
        InsertUserResult InsertUser(Identity identity, User user);

        [OperationContract]
        User[] GetUsersByIds(Identity identity, long[] ids);
    }
}
