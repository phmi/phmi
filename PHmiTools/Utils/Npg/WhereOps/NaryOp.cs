using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace PHmiTools.Utils.Npg.WhereOps
{
    public abstract class NaryOp : WhereOp
    {
        private readonly string _delimeter;
        private readonly IWhereOp[] _parameters;
        
        protected NaryOp(string delimeter, params IWhereOp[] parameters)
        {
            _delimeter = " " + delimeter + " ";
            _parameters = parameters;
        }

        public override string Build(IList<NpgsqlParameter> parameters)
        {
            return string.Join(_delimeter, _parameters.Select(p => string.Format("({0})", p.Build(parameters))));
        }
    }
}
