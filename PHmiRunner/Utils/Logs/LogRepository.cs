using System;
using Npgsql;
using PHmiClient.Logs;
using PHmiClient.Utils.Pagination;
using PHmiTools.Utils.Npg;
using System.Linq;
using PHmiTools.Utils.Npg.WhereOps;

namespace PHmiRunner.Utils.Logs
{
    public class LogRepository : ILogRepository
    {
        private readonly string _tableName;
        private readonly INpgHelper _npgHelper = new NpgHelper();
        private readonly INpgQueryHelper _npgQueryHelper = new NpgQueryHelper();
        public const int MaxItemsToDeletePerTime = 3;
        public const int MaxItemsToRetrieve = 1000;

        private static class DbStr
        {
            public const string Time = "time";
            public const string Text = "text";
            public const string Bytes = "bytes";
        }

        private readonly string[] _columns = new[]
            {
                DbStr.Time,
                DbStr.Text,
                DbStr.Bytes
            };

        private readonly string[] _columnsWithoutBytes = new[]
            {
                DbStr.Time,
                DbStr.Text
            };

        public LogRepository(int logId)
        {
            _tableName = "log_" + logId;
        }

        public void EnsureTable(NpgsqlConnection connection)
        {
            var tables = _npgHelper.GetTables(connection);
            if (tables.Contains(_tableName))
                return;
            var tb = new NpgTableInfoBuilder(_tableName);
            tb.AddColumn(DbStr.Time, NpgDataType.Int8, true);
            tb.AddColumn(DbStr.Text, NpgDataType.Text);
            tb.AddColumn(DbStr.Bytes, NpgDataType.Bytea);
            tb.AddPrimaryKey(DbStr.Time);
            var createTableQuery = _npgQueryHelper.CreateTable(tb.Build());
            _npgHelper.ExecuteScript(connection, new[] { createTableQuery });
        }

        public bool Update(NpgsqlConnection connection, LogItem item)
        {
            var query = _npgQueryHelper.UpdateWhere(
                _tableName,
                new Eq(DbStr.Time, item.Time.Ticks),
                new[] { DbStr.Text, DbStr.Bytes },
                new object[] { item.Text, item.Bytes });
            return _npgHelper.ExecuteNonQuery(connection, query) == 1;
        }

        public void Insert(NpgsqlConnection connection, LogItem item)
        {
            var values = new object[]
                {
                    item.Time.Ticks,
                    item.Text,
                    item.Bytes
                };
            var query = _npgQueryHelper.Insert(
                _tableName,
                _columns,
                new[] {values});
            _npgHelper.ExecuteNonQuery(connection, query);
        }

        public void Delete(NpgsqlConnection connection, DateTime time)
        {
            var query = _npgQueryHelper.DeleteWhere(
                _tableName,
                new Eq(DbStr.Time, time.Ticks));
            _npgHelper.ExecuteNonQuery(connection, query);
        }

        public void DeleteOld(NpgsqlConnection connection, DateTime oldStartTime)
        {
            var selectQuery = _npgQueryHelper.Select(
                _tableName,
                new[] { DbStr.Time },
                new Le(DbStr.Time, oldStartTime.Ticks),
                new[] { DbStr.Time },
                true,
                MaxItemsToDeletePerTime,
                true);
            var query = _npgQueryHelper.DeleteWhere(
                _tableName,
                new In(DbStr.Time, selectQuery));
            _npgHelper.ExecuteNonQuery(connection, query);
        }

        public LogItem[] GetItems(
            NpgsqlConnection connection, CriteriaType criteriaType, DateTime criteria, int maxCount, bool includeBytes)
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
                    whereOp = new Lt(DbStr.Time, criteria.Ticks);
                    asc = false;
                    break;
                case CriteriaType.DownFromOrEqual:
                    whereOp = new Le(DbStr.Time, criteria.Ticks);
                    asc = false;
                    break;
                case CriteriaType.UpFromInfinity:
                    whereOp = null;
                    asc = true;
                    break;
                case CriteriaType.UpFrom:
                    whereOp = new Gt(DbStr.Time, criteria.Ticks);
                    asc = true;
                    break;
                case CriteriaType.UpFromOrEqual:
                    whereOp = new Ge(DbStr.Time, criteria.Ticks);
                    asc = true;
                    break;
                default:
                    throw new NotSupportedException("CriteriaType " + criteriaType);
            }
            var columns = includeBytes ? _columns : _columnsWithoutBytes;
            var query = _npgQueryHelper.Select(
                _tableName, columns, whereOp, new[] { DbStr.Time }, asc, Math.Min(maxCount, MaxItemsToRetrieve));
            var alarms = _npgHelper.ExecuteReader(connection, query, reader =>
                {
                    var i = new LogItem
                        {
                            Time = reader.GetDateTimeFormTicks(0),
                            Text = reader.GetNullableString(1)
                        };
                    if (includeBytes)
                    {
                        i.Bytes = reader.GetByteArray(2);
                    }
                    return i;
                });
            return asc ? alarms.Reverse().ToArray() : alarms;
        }
    }
}
