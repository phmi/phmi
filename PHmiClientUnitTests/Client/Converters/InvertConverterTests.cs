using NUnit.Framework;
using PHmiClient.Converters;

namespace PHmiClientUnitTests.Client.Converters
{
    [TestFixture]
    public class InvertConverterTests
    {
        [Test]
        public void ConvertReturnsTrueIfValueIsFalse()
        {
            var conv = new InvertConverter();
            Assert.AreEqual(true, conv.Convert(false, null, null, null));
        }

        [Test]
        public void ConvertReturnsFalseIfValueIsTrue()
        {
            var conv = new InvertConverter();
            Assert.AreEqual(false, conv.Convert(true, null, null, null));
        }

        [Test]
        public void ConvertReturnsNullIfValueIsNull()
        {
            var conv = new InvertConverter();
            Assert.IsNull(conv.Convert(null, null, null, null));
        }

        [Test]
        public void ConvertBackReturnsTrueIfValueIsFalse()
        {
            var conv = new InvertConverter();
            Assert.AreEqual(true, conv.ConvertBack(false, null, null, null));
        }

        [Test]
        public void ConvertBackReturnsFalseIfValueIsTrue()
        {
            var conv = new InvertConverter();
            Assert.AreEqual(false, conv.ConvertBack(true, null, null, null));
        }

        [Test]
        public void ConvertBackReturnsNullIfValueIsNull()
        {
            var conv = new InvertConverter();
            Assert.IsNull(conv.ConvertBack(null, null, null, null));
        }
    }
}
