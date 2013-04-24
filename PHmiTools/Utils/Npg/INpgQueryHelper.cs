using System.Collections.Generic;
using PHmiTools.Utils.Npg.WhereOps;

namespace PHmiTools.Utils.Npg
{
    public interface INpgQueryHelper
    {
        NpgQuery CreateTable(NpgTableInfo tableInfo);

        NpgQuery CreateColumn(string table, NpgColumnInfo column);

        NpgQuery CreateIndex(string table, NpgIndexType indexType = NpgIndexType.Btree, bool unique = false, params string[] columns);

        NpgQuery Select(
            string table, string[] columnsToReturn,
            IWhereOp whereOp = null,
            string[] columnsOfOrder = null,
            bool asc = true,
            int limit = -1,
            bool distinct = false);

        NpgQuery Insert(string table, string[] columns, IEnumerable<object[]> values);

        NpgQuery UpdateWhere(string table, IWhereOp whereOp, string[] columns, object[] values);

        NpgQuery DeleteWhere(string table, IWhereOp whereOp);
    }
}
