using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using PHmiTools.Utils.Npg.WhereOps;

namespace PHmiTools.Utils.Npg
{
    public class NpgQueryHelper : INpgQueryHelper
    {
        private static string Quote(string field)
        {
            return string.Format("\"{0}\"", field);
        }

        public NpgQuery CreateTable(NpgTableInfo tableInfo)
        {
            var text = new StringBuilder();
            text.AppendFormat("CREATE TABLE {0} (", tableInfo.Name);
            foreach (var columnInfo in tableInfo.Columns)
            {
                text.AppendFormat("{0} {1} {2},",
                    columnInfo.Name,
                    columnInfo.DataType,
                    columnInfo.NotNull ? "NOT NULL" : "NULL");
            }
            text.AppendFormat("CONSTRAINT pk_{0} PRIMARY KEY ({1})",
                tableInfo.Name,
                string.Join(", ", tableInfo.PrimaryKey));
            text.AppendFormat(")");
            return new NpgQuery(text.ToString());
        }

        public NpgQuery CreateColumn(string table, NpgColumnInfo column)
        {
            var text = string.Format("ALTER TABLE {0} ADD COLUMN {1} {2} {3};",
                table, column.Name, column.DataType,
                column.NotNull ? "NOT NULL" : "NULL");
            return new NpgQuery(text);
        }

        public NpgQuery CreateIndex(string table, NpgIndexType indexType, bool unique, params string[] columns)
        {
            return new NpgQuery(string.Format("CREATE {0}INDEX i_{1}_{2} ON {1} USING {3} ({4})",
                 unique ? "UNIQUE " : string.Empty,
                 table,
                 string.Join("_", columns),
                 GetIndexType(indexType),
                 string.Join(", ", columns)));
        }

        private static string GetIndexType(NpgIndexType indexType)
        {
            switch (indexType)
            {
                case NpgIndexType.Btree:
                    return "BTREE";
                default:
                    throw new NotSupportedException(string.Format("NpgIndexType " + indexType));
            }
        }

        public string Select(
            IList<NpgsqlParameter> parameters,
            string table, string[] columnsToReturn,
            IWhereOp whereOp,
            string[] columnsOfOrder,
            bool asc,
            int limit,
            bool distinct)
        {
            var text = new StringBuilder();
            text.Append("SELECT ");
            if (distinct)
            {
                text.Append("DISTINCT ");
            }
            text.AppendFormat(
                "{0} FROM {1}",
                string.Join(", ", columnsToReturn.Select(Quote)),
                Quote(table));
            if (whereOp != null)
            {
                text.AppendFormat(" WHERE {0}", whereOp.Build(parameters));
            }
            if (columnsOfOrder != null)
            {
                var sortDirection = asc ? " ASC" : " DESC";
                text.AppendFormat(
                    " ORDER BY {0}",
                    string.Join(", ", columnsOfOrder.Select(c => Quote(c) + sortDirection)));
            }
            if (limit != -1)
            {
                text.AppendFormat(" LIMIT {0}", limit);
            }
            return text.ToString();
        }

        public NpgQuery Union(ICollection<NpgsqlParameter> parameters,
            IEnumerable<string> selectTexts,
            string[] columnsOfOrder,
            bool asc,
            int limit)
        {
            var text = new StringBuilder();
            text.Append(string.Join(" UNION ", selectTexts.Select(t => "(" + t + ")")));
            if (columnsOfOrder != null)
            {
                var sortDirection = asc ? " ASC" : " DESC";
                text.AppendFormat(
                    " ORDER BY {0}",
                    string.Join(", ", columnsOfOrder.Select(c => Quote(c) + sortDirection)));
            }
            if (limit != -1)
            {
                text.AppendFormat(" LIMIT {0}", limit);
            }
            return new NpgQuery(text.ToString(), parameters.ToArray());
        }

        public NpgQuery Select(
            string table, string[] columnsToReturn,
            IWhereOp whereOp,
            string[] columnsOfOrder,
            bool asc,
            int limit,
            bool distinct)
        {

            var parameters = new List<NpgsqlParameter>();
            var text = Select(parameters, table, columnsToReturn, whereOp, columnsOfOrder, asc, limit, distinct);
            return new NpgQuery(text, parameters.ToArray());
        }

        public NpgQuery Insert(string table, string[] columns, IEnumerable<object[]> values)
        {
            var valuesArray = values as object[][] ?? values.ToArray();
            var parameters = new NpgsqlParameter[columns.Length * valuesArray.Count()];
            var insertDataScript = new string[valuesArray.Count()];
            var parametersIndex = 0;
            for (var i = 0; i < valuesArray.Length; i++)
            {
                var i1 = i;
                insertDataScript[i] = string.Format(
                    "({0})",
                    string.Join(", ", columns.Select(c => string.Format(":{0}", c + i1))));
                var data = valuesArray[i];
                for (var j = 0; j < columns.Length; j++)
                {
                    var column = columns[j];
                    var value = data[j];
                    parameters[parametersIndex++] = new NpgsqlParameter(column + i1, value);
                }
            }
            var scriptRow = string.Format(
                "INSERT INTO {0} ({1}) VALUES {2}",
                Quote(table),
                string.Join(", ", columns.Select(Quote)),
                string.Join(", ", insertDataScript));
            return new NpgQuery(scriptRow, parameters);
        }

        public const string SetParameterNamePrefix = "set_";

        public NpgQuery UpdateWhere(string table, IWhereOp whereOp, string[] columns, object[] values)
        {
            var setParameters = new NpgsqlParameter[columns.Length];
            var set = new string[columns.Length];
            for (var i = 0; i < set.Length; i++)
            {
                var column = columns[i];
                var parameterName = SetParameterNamePrefix + column;
                set[i] = Quote(column) + " = :" + parameterName;
                setParameters[i] = new NpgsqlParameter(parameterName, values[i]);
            }
            var parameters = new List<NpgsqlParameter>();
            var scriptRow = string.Format(
                "UPDATE {0} SET {1} WHERE {2}",
                Quote(table),
                string.Join(", ", set),
                whereOp.Build(parameters));
            parameters.AddRange(setParameters);
            return new NpgQuery(scriptRow, parameters.ToArray());
        }

        public NpgQuery DeleteWhere(string table, IWhereOp whereOp)
        {
            var parameters = new List<NpgsqlParameter>();
            var text = string.Format("DELETE FROM {0} WHERE {1}",
                Quote(table),
                whereOp.Build(parameters));
            return new NpgQuery(text, parameters.ToArray());
        }
    }
}
