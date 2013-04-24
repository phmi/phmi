using System;
using NUnit.Framework;
using PHmiClient.Converters.MultiValueConverters;

namespace PHmiClientUnitTests.Client.Converters.MultiValueConverters
{
    class OrConverterTests
    {
        [TestCase(true, true, Result = true)]
        [TestCase(true, false, Result = true)]
        [TestCase(false, true, Result = true)]
        [TestCase(false, false, Result = false)]
        public bool ConvertTest(object arg1, object arg2)
        {
            var converter = new OrConverter();
            return (bool)converter.Convert(new[] { arg1, arg2 }, null, null, null);
        }

        [Test]
        public void ConvertBackTest()
        {
            var converter = new OrConverter();
            Assert.Throws<NotSupportedException>(() => converter.ConvertBack(null, null, null, null));
        }
    }
}
