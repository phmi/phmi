namespace PHmiClient.Utils
{
    public interface IConnectionStringHelper
    {
        void Set(string name, string connectionString);

        string Get(string name);

        void Protect();
    }
}
