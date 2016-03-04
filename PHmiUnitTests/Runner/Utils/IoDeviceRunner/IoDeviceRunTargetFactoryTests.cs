using Moq;
using NUnit.Framework;
using PHmiClient.Utils;
using PHmiClientUnitTests;
using PHmiModel;
using PHmiRunner.Utils.IoDeviceRunner;

namespace PHmiUnitTests.Runner.Utils.IoDeviceRunner
{
    public class WhenUsingIoDeviceRunTargetFactory : Specification
    {
        protected IIoDeviceRunTargetFactory Factory;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            Factory = new IoDeviceRunTargetFactory();
        }

        public class AndCallingCreate : WhenUsingIoDeviceRunTargetFactory
        {
            protected Mock<ITimeService> TimeService;
            protected IIoDeviceRunTarget Target;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                TimeService = new Mock<ITimeService>();
                Target = Factory.Create(TimeService.Object, new PHmiModel.Entities.IoDevice { Type = "Type" });
            }

            public class ThenTargetIsCreated : AndCallingCreate
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
