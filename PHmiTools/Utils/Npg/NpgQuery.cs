using System.Data;
using Npgsql;

namespace PHmiTools.Utils.Npg
{
    public class NpgQuery
    {
        public NpgQuery(string text, params NpgsqlParameter[] parameters)
        {
            _text = text;
            _parameters = parameters ?? new NpgsqlParameter[0];
        }

        private readonly string _text;

        public string Text { get { return _text; } }

        private readonly NpgsqlParameter[] _parameters;

        public NpgsqlParameter[] Parameters
        {
            get
            {
                if (_parameters != null)
                {
                    foreach (var p in _parameters)
                    {
                        var strValue = p.Value as string;
                        if (strValue != null && string.IsNullOrEmpty(strValue))
                        {
                            p.Value = null;
                            p.DbType = DbType.String;
                        }
                    }
                }
                return _parameters;
            }
        }
    }
}
