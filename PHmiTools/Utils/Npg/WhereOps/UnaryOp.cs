using System.Collections.Generic;
using Npgsql;

namespace PHmiTools.Utils.Npg.WhereOps
{
    public abstract class UnaryOp : WhereOp
    {
        private readonly string _op;
        private readonly string _column;
        private readonly object _value;

        protected UnaryOp(string op, string column, object value)
        {
            _op = op;
            _column = column;
            _value = value;
        }

        public override string Build(IList<NpgsqlParameter> parameters)
        {
            var index = parameters.Count;
            var parameterName = string.Format("{0}{1}", ParameterName, index);
            parameters.Add(new NpgsqlParameter(parameterName, _value));
            return string.Format("\"{0}\" {1} :{2}", _column, _op, parameterName);
        }
    }
}
