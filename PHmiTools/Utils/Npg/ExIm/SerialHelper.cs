using System;
using System.Collections.Generic;
using System.Linq;

namespace PHmiTools.Utils.Npg.ExIm
{
    public class SerialHelper
    {
        private SerialHelper()
        {

        }

        public class SerialHelperItem
        {
            public long MaxValue { get; set; }
            public string Table { get; set; }
            public string Column { get; set; }
        }

        public readonly Dictionary<string, Dictionary<string, SerialHelperItem>> Items
            = new Dictionary<string, Dictionary<string, SerialHelperItem>>();

        public static SerialHelper Create(NpgTableInfo[] tableInfos)
        {
            var helper = new SerialHelper();
            foreach (var tableInfo in tableInfos)
            {
                foreach (var columnInfo in tableInfo.Columns)
                {
                    if (columnInfo.DataType == NpgDataType.Serial
                        || columnInfo.DataType == NpgDataType.Bigserial)
                    {
                        if (!helper.Items.ContainsKey(tableInfo.Name))
                            helper.Items.Add(tableInfo.Name, new Dictionary<string, SerialHelperItem>());
                        helper.Items[tableInfo.Name].Add(columnInfo.Name, new SerialHelperItem
                            {
                                Table = tableInfo.Name,
                                Column = columnInfo.Name
                            });
                    }
                }
            }
            return helper;
        }

        public void Update(TableData tableData)
        {
            Dictionary<string, SerialHelperItem> items;
            if (!Items.TryGetValue(tableData.TableName, out items))
                return;
            for (var i = 0; i < tableData.Columns.Length; i++)
            {
                SerialHelperItem item;
                if (!items.TryGetValue(tableData.Columns[i], out item))
                    continue;
                foreach (var data in tableData.Data)
                {
                    var dataValue = data[i];
                    long value;
                    var longVal = dataValue as long?;
                    if (longVal.HasValue)
                    {
                        value = longVal.Value;
                    }
                    else
                    {
                        value = (int) dataValue;
                    }
                    if (value > item.MaxValue)
                        item.MaxValue = value;
                }
            }
        }

        public NpgQuery[] CreateAlterScriptItem()
        {
            return (from item in Items.Values
                    from serialHelperItem in item.Values
                    select new NpgQuery(string.Format(
                        "ALTER SEQUENCE {0}_{1}_seq RESTART WITH {2}",
                        serialHelperItem.Table,
                        serialHelperItem.Column,
                        serialHelperItem.MaxValue + 1))).ToArray();
        }
    }
}
