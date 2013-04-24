using System.Collections.Generic;
using System.Linq;

namespace PHmiTools.Utils.Npg
{
    public class NpgTableInfoBuilder
    {
        private readonly string _table;
        private readonly IList<NpgColumnInfo> _columns = new List<NpgColumnInfo>();
        private readonly IList<string> _primaryKeys = new List<string>(); 

        public NpgTableInfoBuilder(string table)
        {
            _table = table;
        }

        public void AddColumn(string name, NpgDataType dataType, bool notNull = false)
        {
            _columns.Add(new NpgColumnInfo
                             {
                                 Name = name,
                                 DataType = dataType,
                                 NotNull = notNull
                             });
        }

        public void AddPrimaryKey(string name)
        {
            _primaryKeys.Add(name);
        }

        public NpgTableInfo Build()
        {
            return new NpgTableInfo
                       {
                           Name = _table,
                           Columns = _columns.ToArray(),
                           PrimaryKey = _primaryKeys.ToArray()
                       };
        }
    }
}
