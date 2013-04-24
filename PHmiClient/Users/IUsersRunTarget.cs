using System;
using PHmiClient.PHmiSystem;
using PHmiClient.Utils.Pagination;

namespace PHmiClient.Users
{
    internal interface IUsersRunTarget : IServiceRunTarget
    {
        void LogOn(string name, string password, Action<User> callback);
        void ChangePassword(string name, string oldPassword, string newPassword, Action<bool> callback);
        void GetUsers(Identity identity, CriteriaType criteriaType, string name, int count, Action<User[]> callback);
        void SetPassword(Identity identity, long userId, string password, Action<bool> callback);
        void UpdateUser(Identity identity, User user, Action<UpdateUserResult> callback);
        void InsertUser(Identity identity, User user, Action<InsertUserResult> callback);
        void GetUsers(Identity identity, long[] ids, Action<User[]> callback);
    }
}
