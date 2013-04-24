using System.Collections.Generic;
using Npgsql;

namespace PHmiTools.Utils.Npg.WhereOps
{
    public abstract class WhereOp : IWhereOp
    {
        public const string ParameterName = "value";

        public abstract string Build(IList<NpgsqlParameter> parameters);
    }
}
