using System.Globalization;
using Npgsql;
using PHmiClient.Converters;
using PHmiClient.Users;
using PHmiClient.Utils.Pagination;
using PHmiModel;
using PHmiModel.Interfaces;
using PHmiTools.Utils.Npg;
using System.Linq;

namespace PHmiRunner.Utils.Users
{
    public class UsersRunner : IUsersRunner
    {
        private IModelContext _context;
        private readonly INpgsqlConnectionFactory _connectionFactory;
        private readonly IUsersRepository _repository;

        public UsersRunner(
            IModelContext context,
            INpgsqlConnectionFactory connectionFactory,
            IUsersRepository repository)
        {
            _context = context;
            _connectionFactory = connectionFactory;
            _repository = repository;
        }

        public void Start()
        {
            using (var connection = _connectionFactory.Create())
            {
                _repository.EnsureTable(connection);
                InsertContextUsers(connection);
                _context = null;
            }
        }

        private void InsertContextUsers(NpgsqlConnection connection)
        {
            var users = _context.Get<users>().ToList();
            if (!users.Any())
                return;
            var minId = -users.Select(u => u.id).Max();
            var maxId = -users.Select(u => u.id).Min();
            var repoUsersIds = _repository.GetIds(connection, minId, maxId);
            foreach (var user in repoUsersIds
                .Select(t => users.FirstOrDefault(u => u.id == -t.Item1 || u.name == t.Item2))
                .Where(user => user != null))
            {
                users.Remove(user);
            }
            if (!users.Any())
                return;
            var usersToInsert = users.Select(u => new User
                {
                    Id = -u.id,
                    Name = u.name,
                    Description = u.description,
                    Photo = u.photo,
                    Enabled = u.enabled,
                    CanChange = u.can_change,
                    Privilege = u.privilege
                }).ToArray();
            var usersPasswordsToInsert = users.Select(u => u.password).ToArray();
            _repository.Insert(connection, usersToInsert, usersPasswordsToInsert);
        }

        public void Stop()
        {
        }

        public User LogOn(string name, string password)
        {
            using (var connection = _connectionFactory.Create())
            {
                var users = _repository.GetByNameAndPassword(connection, name, password);
                var user = users.FirstOrDefault();
                if (user != null && user.Enabled)
                    return user;
                return null;
            }
        }

        public bool ChangePassword(string name, string oldPassword, string newPassword)
        {
            using (var connection = _connectionFactory.Create())
            {
                var user = LogOn(name, oldPassword);
                if (user != null && user.CanChange)
                {
                    var result = _repository.SetPassword(connection, user.Id, newPassword);
                    return result;
                }
                return false;
            }
        }

        public int? GetPrivilege(Identity identity)
        {
            if (identity == null)
                return null;

            using (var connection = _connectionFactory.Create())
            {
                var pnp = _repository.GetPrivilegeAndNameAndPassword(connection, identity.UserId);
                if (pnp == null)
                    return null;

                if (!identity.Equals(new Identity(identity.UserId, pnp.Item2, pnp.Item3)))
                    return null;

                return pnp.Item1;
            }
        }

        private bool IsUserPrivileged(Identity identity, int privilege)
        {
            var userPrivilege = GetPrivilege(identity);
            if (!userPrivilege.HasValue)
                return false;
            var userAdminPrivelege = Int32ToPrivilegedConverter.ConvertBack(privilege.ToString(CultureInfo.InvariantCulture));
            if (!userAdminPrivelege.HasValue)
                return false;
            return (userPrivilege.Value & userAdminPrivelege.Value) != 0;
        }

        private bool IsUserViewer(Identity identity)
        {
            return IsUserPrivileged(identity, 31);
        }

        private bool IsUserEditor(Identity identity)
        {
            return IsUserPrivileged(identity, 30);
        }

        public User[] GetUsers(Identity identity, CriteriaType criteriaType, string name, int count)
        {
            if (!IsUserViewer(identity))
                return new User[0];
            using (var connection = _connectionFactory.Create())
            {
                return _repository.Get(connection, criteriaType, name, count);
            }
        }

        public bool SetPassword(Identity identity, long id, string password)
        {
            if (!IsUserEditor(identity))
                return false;
            using (var connection = _connectionFactory.Create())
            {
                return _repository.SetPassword(connection, id, password);
            }
        }

        public UpdateUserResult UpdateUser(Identity identity, User user)
        {
            if (!IsUserEditor(identity))
                return UpdateUserResult.Fail;
            using (var connection = _connectionFactory.Create())
            {
                return _repository.UpdateUser(connection, user);
            }
        }

        public InsertUserResult InsertUser(Identity identity, User user)
        {
            if (!IsUserEditor(identity))
                return InsertUserResult.Fail;
            using (var connection = _connectionFactory.Create())
            {
                return _repository.InsertUser(connection, user);
            }
        }

        public User[] GetUsers(Identity identity, long[] ids)
        {
            if (!IsUserViewer(identity))
                return new User[0];
            using (var connection = _connectionFactory.Create())
            {
                return _repository.Get(connection, ids);
            }
        }
    }
}
