using System;

namespace PHmiTools.Utils.Npg.ExIm
{
    [Serializable]
    public class TableData
    {
        public string TableName;

        public string[] Columns;

        public object[][] Data;
    }
}
