using System;

namespace PHmiClientUnitTests
{
    public class RandomGenerator
    {
        private static readonly Random Random = new Random();

        public static int GetRandomInt32(int min = int.MinValue, int max = int.MaxValue)
        {
            return Random.Next(min, max);
        }
    }
}
