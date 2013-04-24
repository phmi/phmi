using System.Collections.Generic;
using Npgsql;

namespace PHmiTools.Utils.Npg.WhereOps
{
    public class Not : WhereOp
    {
        private readonly IWhereOp _op;

        public Not(IWhereOp op)
        {
            _op = op;
        }

        public override string Build(IList<NpgsqlParameter> parameters)
        {
            return string.Format("not ({0})", _op.Build(parameters));
        }
    }
}
