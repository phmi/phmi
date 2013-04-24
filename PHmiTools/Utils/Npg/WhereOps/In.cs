using System.Collections.Generic;
using Npgsql;

namespace PHmiTools.Utils.Npg.WhereOps
{
    public class In : WhereOp
    {
        private readonly string _column;
        private readonly object[] _values;
        private readonly NpgQuery _selectQuery;

        public In(string column, params object[] values)
        {
            _column = column;
            _values = values;
        }

        public In(string column, NpgQuery selectQuery)
        {
            _column = column;
            _selectQuery = selectQuery;
        }

        public override string Build(IList<NpgsqlParameter> parameters)
        {
            if (_values != null)
            {
                return BuildWithValues(parameters);
            }
            return BuildWithSelectQuery(parameters);
        }

        private string BuildWithSelectQuery(ICollection<NpgsqlParameter> parameters)
        {
            foreach (var p in _selectQuery.Parameters)
            {
                parameters.Add(p);
            }
            return string.Format("{0} in ({1})", _column, _selectQuery.Text);
        }

        private string BuildWithValues(ICollection<NpgsqlParameter> parameters)
        {
            var index = parameters.Count;
            var parameterNames = new string[_values.Length];
            for (var i = 0; i < _values.Length; i++)
            {
                var value = _values[i];
                var parameterName = ":" + ParameterName + index + i;
                parameters.Add(new NpgsqlParameter(parameterName, value));
                parameterNames[i] = parameterName;
            }
            return string.Format("{0} in ({1})", _column, string.Join(", ", parameterNames));
        }
    }
}
