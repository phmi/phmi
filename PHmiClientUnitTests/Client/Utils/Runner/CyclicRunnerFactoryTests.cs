using Moq;
using NUnit.Framework;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Runner;

namespace PHmiClientUnitTests.Client.Utils.Runner
{
    public class WhenUsingCyclicRunnerFactory : Specification
    {
        protected ICyclicRunnerFactory Factory;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            Factory = new CyclicRunnerFactory();
        }

        public class AndCallingCreate : WhenUsingCyclicRunnerFactory
        {
            protected ICyclicRunner Runner;
            protected Mock<IRunTarget> Target;
            protected Mock<INotificationReporter> NotificationReporter;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Target = new Mock<IRunTarget>();
                NotificationReporter = new Mock<INotificationReporter>();
                Target.SetupGet(t => t.Reporter).Returns(NotificationReporter.Object);
                Runner = Factory.Create(Target.Object);
            }

            public class ThenRunnerCreated : AndCallingCreate
            {
                [Test]
                public void Test()
                {
                    Assert.That(Target, Is.Not.Null);
                }
            }
        }
    }
}
