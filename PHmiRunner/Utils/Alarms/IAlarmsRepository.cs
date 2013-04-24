using System;
using Npgsql;
using PHmiClient.Alarms;
using PHmiClient.Utils.Pagination;

namespace PHmiRunner.Utils.Alarms
{
    public interface IAlarmsRepository
    {
        void EnsureTable(NpgsqlConnection connection);
        AlarmSampleId[] GetActiveIds(NpgsqlConnection connection);
        void Insert(NpgsqlConnection connection, Tuple<DateTime, int, DateTime?>[] newAlarms);
        void Update(NpgsqlConnection connection, AlarmSampleId[] alarms, DateTime endTime);
        void Update(NpgsqlConnection connection, AlarmSampleId[] alarms, DateTime acknowledgeTime, long? userId);
        void DeleteNotActive(NpgsqlConnection connection, DateTime oldStartTime);
        Alarm[] GetCurrentAlarms(NpgsqlConnection connection, CriteriaType criteriaType, AlarmSampleId criteria, int maxCount);
        Alarm[] GetHistoryAlarms(NpgsqlConnection connection, CriteriaType criteriaType, AlarmSampleId criteria, int maxCount);
        bool HasActiveAlarms(NpgsqlConnection connection);
        bool HasUnacknowledgedAlarms(NpgsqlConnection connection);
    }
}
