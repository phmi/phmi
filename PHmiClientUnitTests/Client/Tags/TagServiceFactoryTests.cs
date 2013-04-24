using Moq;
using NUnit.Framework;
using PHmiClient.Tags;
using PHmiClient.Utils.Notifications;

namespace PHmiClientUnitTests.Client.Tags
{
    [TestFixture]
    public class TagServiceFactoryTests
    {
        [Test]
        public void Test()
        {
            var factory = new TagServiceFactory();
            var reporter = new Mock<IReporter>();
            var tagService = factory.Create(reporter.Object);
            Assert.That(tagService, Is.Not.Null);
        }
    }
}
