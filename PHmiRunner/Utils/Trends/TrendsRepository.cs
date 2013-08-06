using System;
using Npgsql;
using PHmiClient.Utils.Pagination;
using PHmiTools.Utils.Npg;
using System.Collections.Generic;
using System.Linq;
using PHmiTools.Utils.Npg.WhereOps;

namespace PHmiRunner.Utils.Trends
{
    public class TrendsRepository : ITrendsRepository
    {
        private static class DbStr
        {
            public const string Time = "time";
        }

        public const int MaxSamplesToDeletePerTime = 3;
        public const int MaxSamplesToRetrieve = 1000;

        private readonly NpgsqlConnection _connection;
        private readonly string _tableName;
        private readonly IEnumerable<int> _trendTagIds;
        private readonly INpgHelper _npgHelper = new NpgHelper();
        private readonly INpgQueryHelper _npgQueryHelper = new NpgQueryHelper();

        public TrendsRepository(NpgsqlConnection connection, int categoryId, IEnumerable<int> trendTagIds)
        {
            _connection = connection;
            _tableName = "trends_" + categoryId;
            _trendTagIds = trendTagIds;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        private string GetTableName(int ordinal)
        {
            return _tableName + "_" + ordinal;
        }

        private static string GetColumnName(int trendTagId)
        {
            return "value_" + trendTagId;
        }

        public void EnsureTables()
        {
            var tables = _npgHelper.GetTables(_connection);
            for (var i = 0; i < TrendTableSelector.TablesCount; i++)
            {
                EnsureTable(i, tables);
            }
        }

        private void EnsureTable(int ordinal, IEnumerable<string> tables)
        {
            var tableName = GetTableName(ordinal);
            if (!tables.Contains(tableName))
            {
                var tb = new NpgTableInfoBuilder(tableName);
                tb.AddColumn(DbStr.Time, NpgDataType.Int8, true);
                tb.AddPrimaryKey(DbStr.Time);
                var createTableQuery = _npgQueryHelper.CreateTable(tb.Build());
                _npgHelper.ExecuteScript(_connection, new[] { createTableQuery });
            }
            var columns = _npgHelper.GetColumns(_connection, tableName);
            foreach (var trendTagId in _trendTagIds)
            {
                var column = GetColumnName(trendTagId);
                if (columns.Contains(column))
                    continue;
                var createColumnQuery = _npgQueryHelper.CreateColumn(
                    tableName,
                    new NpgColumnInfo
                        {
                            Name = column,
                            DataType = NpgDataType.Float8,
                            NotNull = false
                        });
                _npgHelper.ExecuteNonQuery(_connection, createColumnQuery);
            }
        }

        public void DeleteOld(DateTime oldTime)
        {
            for (var i = 0; i < TrendTableSelector.TablesCount; i++)
            {
                var tableName = GetTableName(i);
                var selectQuery = _npgQueryHelper.Select(
                    tableName,
                    new[] {DbStr.Time},
                    new Le(DbStr.Time, oldTime.Ticks),
                    new[] {DbStr.Time},
                    true,
                    MaxSamplesToDeletePerTime,
                    true);
                var query = _npgQueryHelper.DeleteWhere(
                    tableName,
                    new In(DbStr.Time, selectQuery));
                _npgHelper.ExecuteNonQuery(_connection, query);
            }
        }

        public void Insert(DateTime time, int tableIndex, Tuple<int, double>[] samples)
        {
            var tableName = GetTableName(tableIndex);
            var columns = new string[samples.Length + 1];
            columns[0] = DbStr.Time;
            for (var i = 0; i < samples.Length; i++)
            {
                columns[i + 1] = GetColumnName(samples[i].Item1);
            }
            var values = new object[samples.Length + 1];
            values[0] = time.Ticks;
            for (var i = 0; i < samples.Length; i++)
            {
                values[i + 1] = samples[i].Item2;
            }
            var query = _npgQueryHelper.Insert(tableName, columns, new [] {values});
            _npgHelper.ExecuteNonQuery(_connection, query);
        }

        public Tuple<DateTime, double?[]>[] GetPage(int[] trendTagIds, CriteriaType criteriaType, DateTime criteria, int maxCount)
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
            var columns = new List<string>(trendTagIds.Length + 1) { DbStr.Time };
            columns.AddRange(trendTagIds.Select(GetColumnName));
            var columnsArr = columns.ToArray();
            var result = new List<Tuple<DateTime, double?[]>>();
            var limit = Math.Min(maxCount, MaxSamplesToRetrieve);
            for (var i = 0; i < TrendTableSelector.TablesCount; i++)
            {
                var query = _npgQueryHelper.Select(
                _tableName + "_" + i, columnsArr, whereOp, new[] { DbStr.Time }, asc, limit);
                var samples = _npgHelper.ExecuteReader(_connection, query, reader =>
                    {
                        var time = reader.GetDateTimeFormTicks(0);
                        var values = new double?[columns.Count - 1];
                        for (var j = 1; j < columns.Count; j++)
                        {
                            values[j - 1] = reader.GetNullableDouble(j);
                        }
                        return new Tuple<DateTime, double?[]>(time, values);
                    });
                result.AddRange(samples);
            }
            return asc
                ? result.OrderBy(s => s.Item1).Take(limit).Reverse().ToArray()
                : result.OrderByDescending(s => s.Item1).Take(limit).ToArray();
        }

        public Tuple<DateTime, double?[]>[] GetSamples(int[] trendTagIds, DateTime startTime, DateTime? endTime, int rarerer)
        {
            var result = new List<Tuple<DateTime, double?[]>>();
            var columns = new List<string>(trendTagIds.Length + 1) { DbStr.Time };
            columns.AddRange(trendTagIds.Select(GetColumnName));
            var columnsArr = columns.ToArray();
            for (var i = 0; i < TrendTableSelector.TablesCount; i++)
            {
                if (rarerer != 0 && rarerer != i + 1)
                    continue;
                var tableName = GetTableName(i);
                IWhereOp whereOp = new Ge(DbStr.Time, startTime.Ticks);
                if (endTime.HasValue)
                {
                    whereOp = new And(whereOp, new Le(DbStr.Time, endTime.Value.Ticks));
                }
                var query = _npgQueryHelper.Select(
                    tableName,
                    columnsArr,
                    whereOp,
                    limit: MaxSamplesToRetrieve);
                result.AddRange(_npgHelper.ExecuteReader(_connection, query, reader =>
                    {
                        var time = reader.GetDateTimeFormTicks(0);
                        var values = new double?[columns.Count - 1];
                        for (var j = 1; j < columns.Count; j++)
                        {
                            values[j - 1] = reader.GetNullableDouble(j);
                        }
                        return new Tuple<DateTime, double?[]>(time, values);
                    }));
            }
            return result.OrderBy(t => t.Item1).ToArray();
        }
    }
}
