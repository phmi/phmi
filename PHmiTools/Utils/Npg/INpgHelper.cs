using System;
using Npgsql;

namespace PHmiTools.Utils.Npg
{
    public interface INpgHelper
    {
        int ExecuteScript(string connectionString, NpgQuery[] script, bool useTransaction = true, Action<int> itemExecutedCallback = null);
        int ExecuteScript(NpgsqlConnection connection, NpgQuery[] script, bool useTransaction = true, Action<int> itemExecutedCallback = null);
        int CreateDatabase(INpgConnectionParameters connectionParameters);
        int CreateDatabase(string connectionString);
        T[] ExecuteReader<T>(string connectionString, NpgQuery query, Func<NpgsqlDataReader, T> convertFunc);
        T[] ExecuteReader<T>(NpgsqlConnection connection, NpgQuery query, Func<NpgsqlDataReader, T> convertFunc);
        int ExecuteNonQuery(NpgsqlConnection connection, NpgQuery query);
        object ExecuteScalar(string connectionString, NpgQuery query);
        bool DatabaseExists(string connectionString);
        string GetDatabase(string connectionString);
        string[] GetDatabases(INpgConnectionParameters connectionParameters);
        string[] GetDatabases(string connectionString);
        string[] GetTables(NpgsqlConnection connection);
        string[] GetColumns(NpgsqlConnection connection, string table);
    }
}
