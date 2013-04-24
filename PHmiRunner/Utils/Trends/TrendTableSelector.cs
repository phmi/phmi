using System;
using PHmiClient.Trends;

namespace PHmiRunner.Utils.Trends
{
    public class TrendTableSelector : ITrendTableSelector
    {
        public const int TablesCount = TrendsService.MaxRarerer + 1;
        private static readonly int[] Tables;
        private int _index;

        static TrendTableSelector()
        {
            var count = (int) Math.Pow(2, TablesCount) - 1;
            Tables = new int[count];
            for (var i = 0; i < count; i++)
            {
                for (var j = TablesCount - 1; j >= 0; j--)
                {
                    var t = (int) Math.Pow(2, j);
                    if ((i + 1)%t != 0)
                        continue;
                    Tables[i] = j;
                    break;
                }
            }
        }


        public int NextTable()
        {
            if (_index >= Tables.Length)
            {
                _index = 0;
            }
            return Tables[_index++];
        }
    }
}
