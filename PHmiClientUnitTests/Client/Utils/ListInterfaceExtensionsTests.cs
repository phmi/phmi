using System;
using NUnit.Framework;
using System.Collections.Generic;
using PHmiClient.Utils;

namespace PHmiClientUnitTests.Client.Utils
{
    public class ListInterfaceExtensionsTests
    {
        [Test]
        public void BinarySearchReturnValueByMsdnSpecification()
        {
            Func<int, int, int> comparer = (i, i1) => i.CompareTo(i1);
            var numbers = new List<int> { 1, 3 };

            // Following the MSDN documentation for List<T>.BinarySearch:
            // http://msdn.microsoft.com/en-us/library/w4e7fxsh.aspx

            // The zero-based index of item in the sorted List(Of T), if item is found;
            var index = numbers.BinarySearch(1, comparer);
            Assert.AreEqual(0, index);

            index = numbers.BinarySearch(3, comparer);
            Assert.AreEqual(1, index);


            // otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item
            index = numbers.BinarySearch(0, comparer);
            Assert.AreEqual(~0, index);

            index = numbers.BinarySearch(2, comparer);
            Assert.AreEqual(~1, index);


            // or, if there is no larger element, the bitwise complement of Count.
            index = numbers.BinarySearch(4, comparer);
            Assert.AreEqual(~numbers.Count, index);
        }
    }
}
