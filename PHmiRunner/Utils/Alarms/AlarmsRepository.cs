using System;
using System.Linq;
using Npgsql;
using PHmiClient.Alarms;
using PHmiClient.Utils.Pagination;
using PHmiTools.Utils.Npg;
using PHmiTools.Utils.Npg.WhereOps;

namespace PHmiRunner.Utils.Alarms
{
    public class AlarmsRepository : IAlarmsRepository
    {
        private static class DbStr
        {
            public const string StartTime = "start_time";
            public const string AlarmId = "alarm_id";
            public const string EndTime = "end_time";
            public const string AcknowledgeTime = "acknowledge_time";
            public const string UserId = "user_id";
        }

        public const int MaxAlarmGenerationsToDeletePerTime = 3;
        public const int MaxAlarmsToRetrieve = 1000;

        private readonly string _tableName;
        private readonly INpgHelper _npgHelper = new NpgHelper();
        private readonly INpgQueryHelper _npgQueryHelper = new NpgQueryHelper();

        private readonly string[] _columns = new[]
            {
                DbStr.StartTime,
                DbStr.AlarmId,
                DbStr.EndTime,
                DbStr.AcknowledgeTime,
                DbStr.UserId
            };

        public AlarmsRepository(int categoryId)
        {
            _tableName = "alarms_" + categoryId;
        }

        public void EnsureTable(NpgsqlConnection connection)
        {
            var tables = _npgHelper.GetTables(connection);
            if (tables.Contains(_tableName))
                return;
            var tb = new NpgTableInfoBuilder(_tableName);
            tb.AddColumn(DbStr.StartTime, NpgDataType.Int8, true);
            tb.AddColumn(DbStr.AlarmId, NpgDataType.Int4, true);
            tb.AddColumn(DbStr.EndTime, NpgDataType.Int8);
            tb.AddColumn(DbStr.AcknowledgeTime, NpgDataType.Int8);
            tb.AddColumn(DbStr.UserId, NpgDataType.Int8);
            tb.AddPrimaryKey(DbStr.StartTime);
            tb.AddPrimaryKey(DbStr.AlarmId);
            var createTableQuery = _npgQueryHelper.CreateTable(tb.Build());
            var createEndTimeIndexQuery = _npgQueryHelper.CreateIndex(_tableName, columns: DbStr.EndTime);
            var createAckTimeIndexQuery = _npgQueryHelper.CreateIndex(_tableName, columns: DbStr.AcknowledgeTime);
            _npgHelper.ExecuteScript(connection, new[] { createTableQuery, createEndTimeIndexQuery, createAckTimeIndexQuery });
        }

        public AlarmSampleId[] GetActiveIds(NpgsqlConnection connection)
        {
            var getActiveQuery = _npgQueryHelper.Select(
                _tableName, new []{DbStr.StartTime, DbStr.AlarmId}, new IsNull(DbStr.EndTime));
            var activeIds = _npgHelper.ExecuteReader(
                connection,
                getActiveQuery,
                reader => new AlarmSampleId(reader.GetDateTimeFormTicks(0), reader.GetInt32(1)));
            return activeIds;
        }

        public void Insert(NpgsqlConnection connection, Tuple<DateTime, int, DateTime?>[] newAlarms)
        {
            var columns = new[]
                {
                    DbStr.StartTime,
                    DbStr.AlarmId,
                    DbStr.AcknowledgeTime
                };
            var values = newAlarms.Select(a => new object[]
                {
                    a.Item1.Ticks,
                    a.Item2,
                    a.Item3.ToNullableTicks()
                }).ToArray();
            _npgHelper.ExecuteNonQuery(connection, _npgQueryHelper.Insert(_tableName, columns, values));
        }

        public void Update(NpgsqlConnection connection, AlarmSampleId[] alarms, DateTime endTime)
        {
            var query = _npgQueryHelper.UpdateWhere(
                _tableName,
                new Or(alarms.Select(a =>
                    new And(new Eq(DbStr.StartTime, a.StartTime.Ticks), new Eq(DbStr.AlarmId, a.AlarmId)))
                    .Cast<IWhereOp>().ToArray()),
                new[] {DbStr.EndTime},
                new object[] {endTime.Ticks});
            _npgHelper.ExecuteNonQuery(connection, query);
        }

        public void Update(NpgsqlConnection connection, AlarmSampleId[] alarms, DateTime acknowledgeTime, long? userId)
        {
            var query = _npgQueryHelper.UpdateWhere(
                _tableName,
                new And(
                    new Or(alarms.Select(a => new And(new Eq(DbStr.StartTime, a.StartTime.Ticks), new Eq(DbStr.AlarmId, a.AlarmId))).Cast<IWhereOp>().ToArray()),
                    new IsNull(DbStr.AcknowledgeTime)),
                new[] { DbStr.AcknowledgeTime, DbStr.UserId },
                new object[] { acknowledgeTime.Ticks, userId });
            _npgHelper.ExecuteNonQuery(connection, query);
        }

        public void DeleteNotActive(NpgsqlConnection connection, DateTime oldStartTime)
        {
            var selectQuery = _npgQueryHelper.Select(
                _tableName,
                new [] {DbStr.EndTime},
                new Le(DbStr.EndTime, oldStartTime.Ticks),
                new [] {DbStr.EndTime},
                true,
                MaxAlarmGenerationsToDeletePerTime,
                true);
            var query = _npgQueryHelper.DeleteWhere(
                _tableName,
                new In(DbStr.EndTime, selectQuery));
            _npgHelper.ExecuteNonQuery(connection, query);
        }

        private Alarm[] GetAlarms(
            NpgsqlConnection connection, IWhereOp alarmsWhereOp, CriteriaType criteriaType, AlarmSampleId criteria, int maxCount)
        {
            IWhereOp whereOp;
            bool asc;
            switch (criteriaType)
            {
                case CriteriaType.DownFromInfinity:
                    whereOp = null;
                    asc = false;
                    break;
                case CriteriaType.DownFrom:
                    whereOp = new Or(
                        new And(new Eq(DbStr.StartTime, criteria.StartTime.Ticks), new Lt(DbStr.AlarmId, criteria.AlarmId)),
                        new Lt(DbStr.StartTime, criteria.StartTime.Ticks));
                    asc = false;
                    break;
                case CriteriaType.DownFromOrEqual:
                    whereOp = new Or(
                        new And(new Eq(DbStr.StartTime, criteria.StartTime.Ticks), new Le(DbStr.AlarmId, criteria.AlarmId)),
                        new Lt(DbStr.StartTime, criteria.StartTime.Ticks));
                    asc = false;
                    break;
                case CriteriaType.UpFromInfinity:
                    whereOp = null;
                    asc = true;
                    break;
                case CriteriaType.UpFrom:
                    whereOp = new Or(
                        new And(new Eq(DbStr.StartTime, criteria.StartTime.Ticks), new Gt(DbStr.AlarmId, criteria.AlarmId)),
                        new Gt(DbStr.StartTime, criteria.StartTime.Ticks));
                    asc = true;
                    break;
                case CriteriaType.UpFromOrEqual:
                    whereOp = new Or(
                        new And(new Eq(DbStr.StartTime, criteria.StartTime.Ticks), new Ge(DbStr.AlarmId, criteria.AlarmId)),
                        new Gt(DbStr.StartTime, criteria.StartTime.Ticks));
                    asc = true;
                    break;
                default:
                    throw new NotSupportedException("CriteriaType " + criteriaType);
            }
            if (alarmsWhereOp != null)
            {
                whereOp = whereOp == null ? alarmsWhereOp : new And(whereOp, alarmsWhereOp);
            }
            var query = _npgQueryHelper.Select(
                _tableName, _columns, whereOp, new[] { DbStr.StartTime, DbStr.AlarmId }, asc, Math.Min(maxCount, MaxAlarmsToRetrieve));
            var alarms = _npgHelper.ExecuteReader(connection, query, reader => 
                new Alarm(reader.GetDateTimeFormTicks(0), reader.GetInt32(1))
                    {
                        EndTime = reader.GetNullableDateTimeFormTicks(2),
                        AcknowledgeTime = reader.GetNullableDateTimeFormTicks(3),
                        UserId = reader.GetNullableInt64(4)
                    });
            return asc ? alarms.Reverse().ToArray() : alarms;
        }

        public Alarm[] GetCurrentAlarms(NpgsqlConnection connection, CriteriaType criteriaType, AlarmSampleId criteria, int maxCount)
        {
            var whereOp = new Or(new IsNull(DbStr.EndTime), new IsNull(DbStr.AcknowledgeTime));
            return GetAlarms(connection, whereOp, criteriaType, criteria, maxCount);
        }

        public Alarm[] GetHistoryAlarms(NpgsqlConnection connection, CriteriaType criteriaType, AlarmSampleId criteria, int maxCount)
        {
            return GetAlarms(connection, null, criteriaType, criteria, maxCount);
        }

        public bool HasActiveAlarms(NpgsqlConnection connection)
        {
            return GetAlarms(connection, new IsNull(DbStr.EndTime), CriteriaType.DownFromInfinity, null, 1).Any();
        }

        public bool HasUnacknowledgedAlarms(NpgsqlConnection connection)
        {
            return GetAlarms(connection, new IsNull(DbStr.AcknowledgeTime), CriteriaType.DownFromInfinity, null, 1).Any();
        }
    }
}
