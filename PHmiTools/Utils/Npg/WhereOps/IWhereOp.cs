using System.Collections.Generic;
using Npgsql;

namespace PHmiTools.Utils.Npg.WhereOps
{
    public interface IWhereOp
    {
        string Build(IList<NpgsqlParameter> parameters);
    }
}
