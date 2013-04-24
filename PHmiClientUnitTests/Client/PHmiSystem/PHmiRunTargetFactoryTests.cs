using Moq;
using NUnit.Framework;
using PHmiClient.PHmiSystem;
using PHmiClient.Utils.Notifications;
using PHmiClient.Wcf;

namespace PHmiClientUnitTests.Client.PHmiSystem
{
    [TestFixture]
    public class PHmiRunTargetFactoryTests
    {
        [Test]
        public void Test()
        {
            var factory = new PHmiRunTargetFactory();
            var reporter = new Mock<INotificationReporter>();
            var clientFactory = new Mock<IServiceClientFactory>();
            var target = factory.Create(reporter.Object, clientFactory.Object);
            Assert.That(target, Is.Not.Null);
        }
    }
}
