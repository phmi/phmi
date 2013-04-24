using System.ComponentModel;

namespace PHmiTools.Utils.Npg
{
    public interface INpgConnectionParameters : INotifyPropertyChanged, IDataErrorInfo
    {
        string Server { get; set; }
        string Port { get; set; }
        string UserId { get; set; }
        string Password { get; set; }
        string Database { get; set; }
        string ConnectionString { get; }
        string ConnectionStringWithoutDatabase { get; }
        void Update(string connectionString);
    }
}
