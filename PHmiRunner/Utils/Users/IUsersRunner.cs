using PHmiClient.Users;
using PHmiClient.Utils.Pagination;
using PHmiClient.Utils.Runner;

namespace PHmiRunner.Utils.Users
{
    public interface IUsersRunner : IRunner
    {
        User LogOn(string name, string password);
        bool ChangePassword(string name, string oldPassword, string newPassword);
        int? GetPrivilege(Identity identity);
        User[] GetUsers(Identity identity, CriteriaType criteriaType, string name, int count);
        bool SetPassword(Identity identity, long id, string password);
        UpdateUserResult UpdateUser(Identity identity, User user);
        InsertUserResult InsertUser(Identity identity, User user);
        User[] GetUsers(Identity identity, long[] ids);
    }
}
