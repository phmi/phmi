using Moq;
using NUnit.Framework;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;

namespace PHmiClientUnitTests.Client.Utils.Notifications
{
    [TestFixture]
    public class NotificationReporterFactoryTests
    {
        [Test]
        public void Test()
        {
            var timeService = new Mock<ITimeService>();
            var factory = new NotificationReporterFactory();
            var reporter = factory.Create(timeService.Object);
            Assert.That(reporter, Is.Not.Null);
        }
    }
}
