using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using Npgsql;

namespace PHmiTools.Utils.Npg.ExIm
{
    public class NpgExImHelper : INpgExImHelper
    {
        public const string BackupFileExt = "back";

        public NpgColumnInfo[] GetColumns(string scriptRow)
        {
            var result = new List<NpgColumnInfo>();
            var regex = new Regex(@"([\w_]+)\s+([\w_]+)\s+(not null|null)");
            var match = regex.Match(scriptRow);
            while (match.Success)
            {
                NpgDataType dataType;
                if (Enum.TryParse(match.Groups[2].ToString(), true, out dataType))
                {
                    result.Add(new NpgColumnInfo
                        {
                            Name = match.Groups[1].ToString(),
                            DataType = dataType,
                            NotNull = match.Groups[3].ToString() == "not null"
                        });
                }
                match = match.NextMatch();
            }
            return result.ToArray();
        }

        public string[] GetPrimaryKey(string scriptRow)
        {
            var result = new List<string>();
            var regex = new Regex(@"constraint\s+([\w_]+)\s+primary\s+key\s+(\(([\w_]+[\,\s]*)+\))");
            var match = regex.Match(scriptRow);
            if (match.Success)
            {
                var keysStr = match.Groups[2].ToString();
                var findKeysRegex = new Regex(@"[\w_]+");
                var findKeysMatch = findKeysRegex.Match(keysStr);
                while (findKeysMatch.Success)
                {
                    result.Add(findKeysMatch.Groups[0].ToString());
                    findKeysMatch = findKeysMatch.NextMatch();
                }
            }
            return result.ToArray();
        }

        public NpgTableInfo GetTable(string scriptRow)
        {
            var regex = new Regex(@"create\s+table\s+([\w_]+)\s+\(\s*([\w_\s\,\(\)]*)\)");
            var match = regex.Match(scriptRow);
            if (match.Success)
            {
                var tableName = match.Groups[1].ToString();
                var body = match.Groups[2].ToString();
                return new NpgTableInfo
                    {
                        Name = tableName,
                        Columns = GetColumns(body),
                        PrimaryKey = GetPrimaryKey(body)
                    };
            }
            return null;
        }

        public NpgScriptConstraint GetConstraint(string scriptRow)
        {
            var regex = new Regex(
                    @"alter\s+table\s+([\w_]+)\s+add\s+constraint\s+([\w_]+)\s+foreign\s+key\s+"
                    + @"\(([\w_]+)\)\s+references\s+([\w_]+)\s\(([\w_]+)\)");
            var match = regex.Match(scriptRow);
            if (match.Success)
            {
                return new NpgScriptConstraint
                {
                    TableName = match.Groups[1].ToString(),
                    ReferencedTable = match.Groups[4].ToString()
                };
            }
            return null;
        }

        public NpgTableInfo[] GetTables(string[] script)
        {
            var tables = new List<NpgTableInfo>();
            var constraints = new List<NpgScriptConstraint>();
            foreach (var row in script)
            {
                var table = GetTable(row);
                if (table != null)
                {
                    tables.Add(table);
                }
                else
                {
                    var constraint = GetConstraint(row);
                    if (constraint != null)
                    {
                        constraints.Add(constraint);
                    }
                }
            }
            for (var i = 0; i < tables.Count; i++)
            {
                var breakCycle = true;
                for (var tableIndex = 0; tableIndex < tables.Count; tableIndex++)
                {
                    var table = tables[tableIndex];
                    var constraint = constraints.FirstOrDefault(c => c.TableName == table.Name);
                    if (constraint != null)
                    {
                        var referencedTable = tables.FirstOrDefault(t => t.Name == constraint.ReferencedTable);
                        if (referencedTable != null)
                        {
                            var refTableIndex = tables.IndexOf(referencedTable);
                            if (refTableIndex > tableIndex)
                            {
                                tables.RemoveAt(refTableIndex);
                                tables.Insert(tableIndex, referencedTable);
                                breakCycle = false;
                            }
                        }
                    }
                }
                if (breakCycle)
                    break;
            }
            return tables.ToArray();
        }

        public NpgQuery GetSelectScriptItem(NpgTableInfo tableInfo, int maxRows, params KeyValuePair<string, object>[] startParameters)
        {
            var scriptParameters =
                startParameters.Any()
                    ? " WHERE " + string.Join(" AND ", startParameters.Select(p => string.Format("\"{0}\" > :{0}", p.Key)))
                    : string.Empty;
            var scriptRow = string.Format(
                "SELECT {0} FROM {1}{2} ORDER BY {3} LIMIT {4}",
                string.Join(", ", tableInfo.Columns.Select(c => string.Format("\"{0}\"", c.Name))),
                string.Format("\"{0}\"", tableInfo.Name),
                scriptParameters,
                string.Join(", ", tableInfo.PrimaryKey.Select(p => string.Format("\"{0}\"", p))),
                maxRows);
            var parameters =
                startParameters.Any()
                    ? startParameters.Select(p => new NpgsqlParameter(p.Key, p.Value)).ToArray()
                    : null;
            return new NpgQuery(scriptRow, parameters);
        }

        public NpgQuery GetInsertScriptItem(TableData tableData)
        {
            if (tableData.Data.Length == 0)
                return null;
            var parameters = new NpgsqlParameter[tableData.Columns.Length*tableData.Data.Length];
            var insertDataScript = new string[tableData.Data.Length];
            var parametersIndex = 0;
            for (var i = 0; i < tableData.Data.Length; i++)
            {
                var i1 = i;
                insertDataScript[i] = string.Format(
                    "({0})",
                    string.Join(", ", tableData.Columns.Select(c => string.Format(":{0}", c + i1))));
                var data = tableData.Data[i];
                for (var j = 0; j < tableData.Columns.Length; j++)
                {
                    var column = tableData.Columns[j];
                    var value = data[j];
                    parameters[parametersIndex++] = new NpgsqlParameter(column + i1, value);
                }
            }
            var scriptRow = string.Format(
                "INSERT INTO {0} ({1}) VALUES {2}",
                string.Format("\"{0}\"", tableData.TableName),
                string.Join(", ", tableData.Columns.Select(c => string.Format("\"{0}\"", c))),
                string.Join(", ", insertDataScript));
            return new NpgQuery(scriptRow, parameters);
        }

        public BinaryFormatter CreateFormatter()
        {
            var formatter = new BinaryFormatter {AssemblyFormat = FormatterAssemblyStyle.Simple};
            return formatter;
        }
    }
}
