using System;
using Npgsql;
using PHmiClient.Logs;
using PHmiClient.Utils.Pagination;

namespace PHmiRunner.Utils.Logs
{
    public interface ILogRepository
    {
        void EnsureTable(NpgsqlConnection connection);
        bool Update(NpgsqlConnection connection, LogItem item);
        void Insert(NpgsqlConnection connection, LogItem item);
        void Delete(NpgsqlConnection connection, DateTime time);
        void DeleteOld(NpgsqlConnection connection, DateTime oldStartTime);
        LogItem[] GetItems(
            NpgsqlConnection connection, CriteriaType criteriaType, DateTime criteria,
            int maxCount, bool includeBytes);
    }
}
