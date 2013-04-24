using System;
using Moq;
using NUnit.Framework;
using PHmiClient.PHmiSystem;
using PHmiClient.Utils;
using PHmiClient.Wcf;

namespace PHmiClientUnitTests.Client.PHmiSystem
{
    public class WhenUsingAfterUpdateRunTarget : Specification
    {
        internal IEventRunTarget AfterUpdateRunTarget;
        protected Mock<IDispatcherService> DispatcherService;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            DispatcherService = new Mock<IDispatcherService>();
            DispatcherService
                .Setup(service => service.Invoke(It.IsAny<Action>()))
                .Callback<Action>(action => action.Invoke());
            AfterUpdateRunTarget = new EventRunTarget(DispatcherService.Object);
        }

        public class AndRegisteredRunned : WhenUsingAfterUpdateRunTarget
        {
            protected int Count;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Count = 0;
                AfterUpdateRunTarget.Runned += (sender, args) =>
                    {
                        Count++;
                    };
            }

            public class AndRunned : AndRegisteredRunned
            {
                internal Mock<IService> Service;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Service = new Mock<IService>();
                    AfterUpdateRunTarget.Run(Service.Object);
                }

                public class ThenRunnedRaised : AndRunned
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Count, Is.EqualTo(1));
                    }
                }
            }
        }
    }
}
