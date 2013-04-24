using PHmiTools.Utils.Npg;

namespace PHmiTools.Utils
{
    public interface IPHmiDatabaseHelper
    {
        bool IsPHmiDatabase(string connectionString, string database);
        bool IsPHmiDatabase(INpgConnectionParameters connectionParameters);
    }
}
