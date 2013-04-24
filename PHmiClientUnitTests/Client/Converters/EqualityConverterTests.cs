using NUnit.Framework;
using PHmiClient.Converters;

namespace PHmiClientUnitTests.Client.Converters
{
    [TestFixture]
    public class EqualityConverterTests
    {
        private EqualityConverter _converter;

        [SetUp]
        public void SetUp()
        {
            _converter = new EqualityConverter();
        }

        [Test]
        public void StructEqual()
        {
            Assert.IsTrue((bool)_converter.Convert(10, null, 10, null));
        }

        [Test]
        public void StructNotEqual()
        {
            Assert.IsFalse((bool)_converter.Convert(10, null, 11, null));
        }

        [Test]
        public void BothNulls()
        {
            Assert.IsTrue((bool)_converter.Convert(null, null, null, null));
        }

        [Test]
        public void ValueNotNull()
        {
            Assert.IsFalse((bool)_converter.Convert(null, null, new object(), null));
        }

        [Test]
        public void ParameterNotNull()
        {
            Assert.IsFalse((bool)_converter.Convert(new object(), null, null, null));
        }

        [Test]
        public void BothNotNull()
        {
            Assert.IsFalse((bool)_converter.Convert(new object(), null, new object(), null));
        }

        [Test]
        public void BothNotNullEqaul()
        {
            var obj = new object();
            Assert.IsTrue((bool)_converter.Convert(obj, null, obj, null));
        }
    }
}
