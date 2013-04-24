using PHmiModel.Interfaces;

namespace PHmiRunner.Utils.Users
{
    public interface IUsersRunnerFactory
    {
        IUsersRunner Create(IModelContext context, string connectionString);
    }
}
