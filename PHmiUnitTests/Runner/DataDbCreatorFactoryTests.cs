using Moq;
using NUnit.Framework;
using PHmiClient.Utils.Notifications;
using PHmiRunner.Utils;

namespace PHmiUnitTests.Runner
{
    [TestFixture]
    public class DataDbCreatorFactoryTests
    {
        [Test]
        public void Test()
        {
            const string connectionString = "ConnectionString";
            var creatorFactory = new DataDbCreatorFactory();
            var reporter = new Mock<IReporter>();
            var creator = creatorFactory.Create(connectionString, reporter.Object);
            Assert.That(creator, Is.Not.Null);
        }
    }
}
