using System;
using Npgsql;
using PHmiClient.Users;
using PHmiClient.Utils.Pagination;

namespace PHmiRunner.Utils.Users
{
    public interface IUsersRepository
    {
        void EnsureTable(NpgsqlConnection connection);
        Tuple<long, string>[] GetIds(NpgsqlConnection connection, long minId, long maxId);
        void Insert(NpgsqlConnection connection, User[] users, string[] passwords);
        User[] GetByNameAndPassword(NpgsqlConnection connection, string name, string password);
        Tuple<int?, string, string> GetPrivilegeAndNameAndPassword(NpgsqlConnection connection, long id);
        bool SetPassword(NpgsqlConnection connection, long id, string newPassword);
        User[] Get(NpgsqlConnection connection, CriteriaType criteriaType, string name, int count);
        UpdateUserResult UpdateUser(NpgsqlConnection connection, User user);
        InsertUserResult InsertUser(NpgsqlConnection connection, User user);
        User[] Get(NpgsqlConnection connection, long[] ids);
    }
}
