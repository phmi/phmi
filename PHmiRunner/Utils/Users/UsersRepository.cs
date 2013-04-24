using System;
using System.Linq;
using Npgsql;
using PHmiClient.Users;
using PHmiClient.Utils.Pagination;
using PHmiTools.Utils.Npg;
using PHmiTools.Utils.Npg.WhereOps;

namespace PHmiRunner.Utils.Users
{
    public class UsersRepository : IUsersRepository
    {
        private static class DbStr
        {
            public const string Users = "users";
            public const string Id = "id";
            public const string Name = "name";
            public const string Description = "description";
            public const string Photo = "photo";
            public const string Password = "password";
            public const string Enabled = "enabled";
            public const string CanChange = "can_change";
            public const string Privilege = "privilege";
        }

        private readonly INpgHelper _npgHelper = new NpgHelper();
        private readonly INpgQueryHelper _npgQueryHelper = new NpgQueryHelper();
        private readonly string[] _userColumns = new[]
            {
                DbStr.Id,
                DbStr.Name,
                DbStr.Description,
                DbStr.Photo,
                DbStr.Enabled,
                DbStr.CanChange,
                DbStr.Privilege
            };

        public void EnsureTable(NpgsqlConnection connection)
        {
            var tables = _npgHelper.GetTables(connection);
            if (tables.Contains(DbStr.Users))
                return;
            var tb = new NpgTableInfoBuilder(DbStr.Users);
            tb.AddColumn(DbStr.Id, NpgDataType.Int8, true);
            tb.AddColumn(DbStr.Name, NpgDataType.Text, true);
            tb.AddColumn(DbStr.Description, NpgDataType.Text, true);
            tb.AddColumn(DbStr.Photo, NpgDataType.Bytea);
            tb.AddColumn(DbStr.Password, NpgDataType.Text);
            tb.AddColumn(DbStr.Enabled, NpgDataType.Bool, true);
            tb.AddColumn(DbStr.CanChange, NpgDataType.Bool, true);
            tb.AddColumn(DbStr.Privilege, NpgDataType.Int4);
            tb.AddPrimaryKey(DbStr.Id);
            var createUsersTableQuery = _npgQueryHelper.CreateTable(tb.Build());
            var createNameIndexQuery = _npgQueryHelper.CreateIndex(DbStr.Users, unique: true, columns: DbStr.Name);
            _npgHelper.ExecuteScript(connection, new[] { createUsersTableQuery, createNameIndexQuery });
        }

        public Tuple<long, string>[] GetIds(NpgsqlConnection connection, long minId, long maxId)
        {
            var getRepoUsersIdsQuery = _npgQueryHelper
                .Select(DbStr.Users, new []{DbStr.Id, DbStr.Name}, new And(new Ge(DbStr.Id, minId), new Le(DbStr.Id, maxId)));
            var repoUsersIds = _npgHelper.ExecuteReader(
                connection,
                getRepoUsersIdsQuery,
                reader => new Tuple<long, string>(reader.GetInt64(0), reader.GetString(1)));
            return repoUsersIds;
        }

        public void Insert(NpgsqlConnection connection, User[] users, string[] passwords)
        {
            var columns = new[]
                {
                    DbStr.Id,
                    DbStr.Name,
                    DbStr.Description,
                    DbStr.Photo,
                    DbStr.Password,
                    DbStr.Enabled,
                    DbStr.CanChange,
                    DbStr.Privilege
                };
            var values = users.Select((u, i) => new object[]
                {
                    u.Id,
                    u.Name,
                    u.Description,
                    u.Photo,
                    passwords[i],
                    u.Enabled,
                    u.CanChange,
                    u.Privilege
                }).ToArray();
            _npgHelper.ExecuteNonQuery(connection, _npgQueryHelper.Insert(DbStr.Users, columns, values));
        }

        public User[] GetByNameAndPassword(NpgsqlConnection connection, string name, string password)
        {
            var query = _npgQueryHelper.Select(
                    DbStr.Users, _userColumns, new And(new Eq(DbStr.Name, name), new Eq(DbStr.Password, password)));
            var users = _npgHelper.ExecuteReader(connection, query, GetUser);
            return users;
        }

        public Tuple<int?, string, string> GetPrivilegeAndNameAndPassword(NpgsqlConnection connection, long id)
        {
            var query = _npgQueryHelper.Select(
                    DbStr.Users, new []{ DbStr.Privilege, DbStr.Name, DbStr.Password }, new Eq(DbStr.Id, id));
            var cridentials = _npgHelper.ExecuteReader(connection, query,
                reader => new Tuple<int?, string, string>(reader.GetNullableInt32(0), reader.GetString(1), reader.GetNullableString(2)));
            return cridentials.SingleOrDefault();
        }

        public bool SetPassword(NpgsqlConnection connection, long id, string newPassword)
        {
            var query = _npgQueryHelper.UpdateWhere(
                DbStr.Users, new Eq(DbStr.Id, id), new[] {DbStr.Password}, new object[] {newPassword});
            var result = _npgHelper.ExecuteNonQuery(connection, query);
            return result == 1;
        }

        private static User GetUser(NpgsqlDataReader reader)
        {
            return new User
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2),
                    Photo = reader.GetByteArray(3),
                    Enabled = reader.GetBoolean(4),
                    CanChange = reader.GetBoolean(5),
                    Privilege = reader.GetNullableInt32(6)
                };
        }

        public const int MaxUsersToRetrieve = 50;

        public User[] Get(NpgsqlConnection connection, CriteriaType criteriaType, string name, int count)
        {
            IWhereOp whereOp;
            bool asc;
            switch (criteriaType)
            {
                case CriteriaType.DownFromInfinity:
                    whereOp = null;
                    asc = true;
                    break;
                case CriteriaType.DownFrom:
                    whereOp = new Gt(DbStr.Name, name);
                    asc = true;
                    break;
                case CriteriaType.DownFromOrEqual:
                    whereOp = new Ge(DbStr.Name, name);
                    asc = true;
                    break;
                case CriteriaType.UpFromInfinity:
                    whereOp = null;
                    asc = false;
                    break;
                case CriteriaType.UpFrom:
                    whereOp = new Lt(DbStr.Name, name);
                    asc = false;
                    break;
                case CriteriaType.UpFromOrEqual:
                    whereOp = new Le(DbStr.Name, name);
                    asc = false;
                    break;
                default:
                    throw new NotSupportedException("CriteriaType " + criteriaType);
            }
            var limit = Math.Min(count, MaxUsersToRetrieve);
            var query = _npgQueryHelper.Select(
                DbStr.Users, _userColumns, whereOp, new[] { DbStr.Name }, asc, limit);
            var users = _npgHelper.ExecuteReader(connection, query, GetUser);
            return asc ? users : users.Reverse().ToArray();
        }

        public UpdateUserResult UpdateUser(NpgsqlConnection connection, User user)
        {
            try
            {
                var count = _npgHelper.ExecuteNonQuery(connection, _npgQueryHelper.UpdateWhere(
                    DbStr.Users, new Eq(DbStr.Id, user.Id),
                    new[]
                        {
                            DbStr.Name,
                            DbStr.Description,
                            DbStr.Photo,
                            DbStr.Enabled,
                            DbStr.CanChange,
                            DbStr.Privilege
                        },
                    new object[]
                        {
                            user.Name,
                            user.Description,
                            user.Photo,
                            user.Enabled,
                            user.CanChange,
                            user.Privilege
                        }));
                return count == 1 ? UpdateUserResult.Success : UpdateUserResult.UserNotFound;
            }
            catch (NpgsqlException exception)
            {
                switch (exception.Code)
                {
                    case "23505":
                        return UpdateUserResult.NameConflict;
                    case "23502":
                        return UpdateUserResult.NullValue;
                    default:
                        return UpdateUserResult.Fail;
                }
            }
        }

        public InsertUserResult InsertUser(NpgsqlConnection connection, User user)
        {
            try
            {
                var count = _npgHelper.ExecuteNonQuery(connection, _npgQueryHelper.Insert(
                    DbStr.Users,
                    new[]
                        {
                            DbStr.Id,
                            DbStr.Name,
                            DbStr.Description,
                            DbStr.Photo,
                            DbStr.Enabled,
                            DbStr.CanChange,
                            DbStr.Privilege
                        },
                    new[]
                        {
                            new object[]
                                {
                                    user.Id,
                                    user.Name,
                                    user.Description,
                                    user.Photo,
                                    user.Enabled,
                                    user.CanChange,
                                    user.Privilege
                                }
                        }));
                return count == 1 ? InsertUserResult.Success : InsertUserResult.Fail;
            }
            catch (NpgsqlException exception)
            {
                switch (exception.Code)
                {
                    case "23505":
                        return exception.Detail.Contains(DbStr.Id) ? InsertUserResult.IdConflict : InsertUserResult.NameConflict;
                    case "23502":
                        return InsertUserResult.NullValue;
                    default:
                        return InsertUserResult.Fail;
                }
            }
        }

        public User[] Get(NpgsqlConnection connection, long[] ids)
        {
            var query = _npgQueryHelper.Select(DbStr.Users, _userColumns, new In(DbStr.Id, ids));
            return _npgHelper.ExecuteReader(connection, query, GetUser);
        }
    }
}
