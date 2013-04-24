using PHmiModel.Interfaces;
using PHmiTools.Utils.Npg;

namespace PHmiRunner.Utils.Users
{
    public class UsersRunnerFactory : IUsersRunnerFactory
    {
        public IUsersRunner Create(IModelContext context, string connectionString)
        {
            return new UsersRunner(context, new NpgsqlConnectionFactory(connectionString), new UsersRepository());
        }
    }
}
