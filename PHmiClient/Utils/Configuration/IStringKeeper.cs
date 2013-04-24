namespace PHmiClient.Utils.Configuration
{
    public interface IStringKeeper
    {
        void Reload();
        string Get(string key);
        void Set(string key, string value);
        void Save();
    }
}
