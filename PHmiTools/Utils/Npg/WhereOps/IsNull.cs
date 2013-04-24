using Npgsql;
using System.Collections.Generic;

namespace PHmiTools.Utils.Npg.WhereOps
{
    public class IsNull : WhereOp
    {
        private readonly string _column;

        public IsNull(string column)
        {
            _column = column;
        }

        public override string Build(IList<NpgsqlParameter> parameters)
        {
            return string.Format("\"{0}\" ISNULL", _column);
        }
    }
}
