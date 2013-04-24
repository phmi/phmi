using Npgsql;
using System.Collections.Generic;

namespace PHmiTools.Utils.Npg.WhereOps
{
    public class NotNull : WhereOp
    {
        private readonly string _column;

        public NotNull(string column)
        {
            _column = column;
        }

        public override string Build(IList<NpgsqlParameter> parameters)
        {
            return string.Format("\"{0}\" NOTNULL", _column);
        }
    }
}
