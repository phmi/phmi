using Moq;
using NUnit.Framework;
using PHmiClient.Tags;

namespace PHmiClientUnitTests.Client.Tags
{
    [TestFixture]
    public class TagBaseTests
    {
        [Test]
        public void CanMockTest()
        {
            var mock = new Mock<TagAbstract<object>>();
            var obj = new object();
            mock.Setup(t => t.GetWrittenValue()).Returns(obj);
            Assert.That(mock.Object.GetWrittenValue(), Is.SameAs(obj));
        }
    }
}
