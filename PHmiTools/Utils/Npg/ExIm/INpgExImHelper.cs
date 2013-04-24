using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace PHmiTools.Utils.Npg.ExIm
{
    public interface INpgExImHelper
    {
        NpgColumnInfo[] GetColumns(string scriptRow);
        string[] GetPrimaryKey(string scriptRow);
        NpgTableInfo GetTable(string scriptRow);
        NpgScriptConstraint GetConstraint(string scriptRow);
        NpgTableInfo[] GetTables(string[] script);
        NpgQuery GetSelectScriptItem(NpgTableInfo tableInfo, int maxRows, params KeyValuePair<string, object>[] startParameters);
        NpgQuery GetInsertScriptItem(TableData tableData);
        BinaryFormatter CreateFormatter();
    }
}
