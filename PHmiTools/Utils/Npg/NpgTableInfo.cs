
namespace PHmiTools.Utils.Npg
{
    public class NpgTableInfo
    {
        public string Name { get; set; }

        public NpgColumnInfo[] Columns { get; set; }

        public string[] PrimaryKey { get; set; }

        public int[] GetPrimaryKeyIndexes()
        {
            var primaryKeyIndexes = new int[PrimaryKey.Length];
            for (var i = 0; i < primaryKeyIndexes.Length; i++)
            {
                var index = -1;
                for (var j = 0; j < Columns.Length; j++)
                {
                    if (Columns[j].Name == PrimaryKey[i])
                    {
                        index = j;
                        break;
                    }
                }
                primaryKeyIndexes[i] = index;
            }
            return primaryKeyIndexes;
        }
    }
}
