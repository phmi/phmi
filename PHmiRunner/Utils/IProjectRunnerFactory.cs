namespace PHmiRunner.Utils
{
    public interface IProjectRunnerFactory
    {
        IProjectRunner Create(string projectName, string connectionString);
    }
}
