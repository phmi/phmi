using System;
using NUnit.Framework;
using PHmiClient.Converters.MultiValueConverters;

namespace PHmiClientUnitTests.Client.Converters.MultiValueConverters
{
    public class AndConverterTests
    {
        [TestCase(true, true, Result = true)]
        [TestCase(true, false, Result = false)]
        [TestCase(false, true, Result = false)]
        [TestCase(false, false, Result = false)]
        public bool ConvertTest(object arg1, object arg2)
        {
            var converter = new AndConverter();
            return (bool) converter.Convert(new [] {arg1, arg2}, null, null, null);
        }

        [Test]
        public void ConvertBackTest()
        {
            var converter = new AndConverter();
            Assert.Throws<NotSupportedException>(() => converter.ConvertBack(null, null, null, null));
        }
    }
}
