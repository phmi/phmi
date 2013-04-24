using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using Npgsql;

namespace PHmiTools.Utils.Npg
{
    public class NpgHelper : INpgHelper
    {
        public int ExecuteScript(
            string connectionString, NpgQuery[] script, bool useTransaction = true, Action<int> itemExecutedCallback = null)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                return ExecuteScript(connection, script, useTransaction, itemExecutedCallback);
            }
        }

        public int ExecuteScript(NpgsqlConnection connection, NpgQuery[] script, bool useTransaction = true, Action<int> itemExecutedCallback = null)
        {
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            var count = 0;
            var transaction = useTransaction ? connection.BeginTransaction() : null;
            for (var row = 0; row < script.Length; row++)
            {
                using (var command = new NpgsqlCommand(script[row].Text, connection))
                {
                    command.Parameters.AddRange(script[row].Parameters);
                    count += command.ExecuteNonQuery();
                    if (itemExecutedCallback != null)
                        itemExecutedCallback.Invoke(row);
                }
            }
            if (useTransaction)
                transaction.Commit();
            return count;
        }

        public int CreateDatabase(INpgConnectionParameters connectionParameters)
        {
            return CreateDatabase(connectionParameters.ConnectionStringWithoutDatabase, connectionParameters.Database);
        }

        public int CreateDatabase(string connectionString)
        {
            var database = GetDatabase(connectionString);
            var connectionsStringWithoutDatabase = GetConnectionStringWithoutDatabase(connectionString);
            return CreateDatabase(connectionsStringWithoutDatabase, database);
        }

        private int CreateDatabase(string connectionStringWithoutDatabase, string database)
        {
            var script = new[]
                {
                    new NpgQuery(string.Format("CREATE DATABASE \"{0}\"", database))
                };
            var count = ExecuteScript(connectionStringWithoutDatabase, script, false);
            Thread.Sleep(1000);
            return count;
        }

        public string GetDatabase(string connectionString)
        {
            return (from part in connectionString.Split(';')
                    select part.Split('=') into values
                    where values.Length == 2 && values[0] == "Database"
                    select values[1]).FirstOrDefault();
        }

        private string GetConnectionStringWithoutDatabase(string connectionStringWithDatabase)
        {
            var database = GetDatabase(connectionStringWithDatabase);
            return connectionStringWithDatabase
                .Replace(string.Format("Database={0};", database), string.Empty)
                .Replace(string.Format("Database={0}", database), string.Empty);
        }

        public T[] ExecuteReader<T>(string connectionString, NpgQuery query, Func<NpgsqlDataReader, T> convertFunc)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                return ExecuteReader(connection, query, convertFunc);
            }
        }

        public T[] ExecuteReader<T>(NpgsqlConnection connection, NpgQuery query, Func<NpgsqlDataReader, T> convertFunc)
        {
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            using (var command = new NpgsqlCommand(query.Text, connection))
            {
                command.Parameters.AddRange(query.Parameters);
                var reader = command.ExecuteReader();
                var result = new List<T>();
                while (reader.Read())
                {
                    result.Add(convertFunc.Invoke(reader));
                }
                return result.ToArray();
            }
        }

        public int ExecuteNonQuery(NpgsqlConnection connection, NpgQuery query)
        {
            if (connection.State == ConnectionState.Closed)
                connection.Open();
            using (var command = new NpgsqlCommand(query.Text, connection))
            {
                command.Parameters.AddRange(query.Parameters);
                var count = command.ExecuteNonQuery();
                return count;
            }
        }

        public bool DatabaseExists(string connectionString)
        {
            var database = GetDatabase(connectionString);
            var databases = GetDatabases(connectionString);
            return databases.Contains(database);
        }

        public string[] GetDatabases(INpgConnectionParameters connectionParameters)
        {
            return GetDatabasesFromConnectionStringWithoutDatabase(connectionParameters.ConnectionStringWithoutDatabase);
        }

        public string[] GetDatabases(string connectionString)
        {
            return GetDatabasesFromConnectionStringWithoutDatabase(GetConnectionStringWithoutDatabase(connectionString));
        }

        private string[] GetDatabasesFromConnectionStringWithoutDatabase(string cs)
        {
            var query = new NpgQuery(
                "SELECT datname FROM pg_database WHERE datallowconn = TRUE AND datname != 'template1' ORDER BY datname");
            return ExecuteReader(
                cs,
                query,
                reader => reader.GetString(0));
        } 

        public object ExecuteScalar(string connectionString, NpgQuery query)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new NpgsqlCommand(query.Text, connection))
                {
                    command.Parameters.AddRange(query.Parameters);
                    return command.ExecuteScalar();
                }
            }
        }

        public string[] GetTables(NpgsqlConnection connection)
        {
            var query = new NpgQuery("SELECT tablename FROM pg_tables");
            return ExecuteReader(connection, query, reader => reader.GetString(0));
        }

        public string[] GetColumns(NpgsqlConnection connection, string table)
        {
            var queryText = string.Format(
                @"SELECT a.attname FROM pg_catalog.pg_attribute a " +
                @"WHERE a.attnum > 0 AND NOT a.attisdropped AND a.attrelid = " +
                @"(SELECT c.oid FROM pg_catalog.pg_class c " +
                @"LEFT JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace " +
                @"WHERE c.relname ~ '^({0})$' AND pg_catalog.pg_table_is_visible(c.oid))", table);
            return ExecuteReader(connection, new NpgQuery(queryText), reader => reader.GetString(0));
        }
    }
}
