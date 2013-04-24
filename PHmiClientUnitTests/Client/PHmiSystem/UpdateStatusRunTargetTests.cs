using System;
using Moq;
using NUnit.Framework;
using PHmiClient.Loc;
using PHmiClient.PHmiSystem;
using PHmiClient.Utils;
using PHmiClient.Wcf;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClientUnitTests.Client.PHmiSystem
{
    public class WhenUsingUpdateStatusRunTarget : Specification
    {
        protected Mock<ITimeService> TimeService;
        internal IUpdateStatusRunTarget UpdateStatusRunTarget;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            TimeService = new Mock<ITimeService>();
            UpdateStatusRunTarget = new UpdateStatusRunTarget(TimeService.Object);
        }

        public class ThenNameReturnsStatusService : WhenUsingUpdateStatusRunTarget
        {
            [Test]
            public void Test()
            {
                Assert.That(UpdateStatusRunTarget.Name, Is.EqualTo(Res.StatusService));
            }
        }

        public class AndRunInvoked : WhenUsingUpdateStatusRunTarget
        {
            internal UpdateStatusResult Result;
            internal Mock<IService> Service;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Service = new Mock<IService>();
                Result = new UpdateStatusResult {Time = DateTime.UtcNow};
                Service.Setup(s => s.UpdateStatus()).Returns(Result);
                UpdateStatusRunTarget.Run(Service.Object);
            }

            public class ThenTimeIsSetted : AndRunInvoked
            {
                [Test]
                public void Test()
                {
                    TimeService.VerifySet(t => t.UtcTime = Result.Time, Times.Once());
                }
            }
        }

        public class AndCleanInvoked : WhenUsingUpdateStatusRunTarget
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                UpdateStatusRunTarget.Clean();
            }

            public class ThenNothingHappens : AndCleanInvoked
            {
                [Test]
                public void Test()
                {
                }
            }
        }
    }
}
