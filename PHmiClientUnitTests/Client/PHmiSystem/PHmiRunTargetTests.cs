using System;
using Moq;
using NUnit.Framework;
using PHmiClient.Loc;
using PHmiClient.PHmiSystem;
using PHmiClient.Utils.Notifications;
using PHmiClient.Wcf;

namespace PHmiClientUnitTests.Client.PHmiSystem
{
    public class WhenUsingPHmiRunTarget : Specification
    {
        protected Mock<INotificationReporter> Reporter;
        internal Mock<IServiceClientFactory> ClientFactory;
        internal Mock<IServiceRunTarget> Target1;
        internal Mock<IServiceRunTarget> Target2;
        internal IPHmiRunTarget PHmiRunTarget;
        internal Mock<IServiceClient> Client;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            Reporter = new Mock<INotificationReporter>();
            ClientFactory = new Mock<IServiceClientFactory>();
            Target1 = new Mock<IServiceRunTarget>();
            Target2 = new Mock<IServiceRunTarget>();
            PHmiRunTarget = new PHmiRunTarget(Reporter.Object, ClientFactory.Object, Target1.Object, Target2.Object);
            Client = new Mock<IServiceClient>();
            ClientFactory.Setup(f => f.Create()).Returns(Client.Object);
        }

        public class ThenNameReturnsPHmiSystem : WhenUsingPHmiRunTarget
        {
            [Test]
            public void Test()
            {
                Assert.That(PHmiRunTarget.Name, Is.EqualTo(Res.PHmi));
            }
        }

        public class ThenReporterReturnsReporter : WhenUsingPHmiRunTarget
        {
            [Test]
            public void Test()
            {
                Assert.That(PHmiRunTarget.Reporter, Is.SameAs(Reporter.Object));
            }
        }

        public class AndInvokingRun : WhenUsingPHmiRunTarget
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                PHmiRunTarget.Run();
            }

            public class ThenClientIsCreated : AndInvokingRun
            {
                [Test]
                public void Test()
                {
                    ClientFactory.Verify(f => f.Create(), Times.Once());
                }
            }

            public class AndInvokingRunAgain : AndInvokingRun
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    PHmiRunTarget.Run();
                }

                public class ThenClientIsNotCreatedAgain : AndInvokingRunAgain
                {
                    [Test]
                    public void Test()
                    {
                        ClientFactory.Verify(f => f.Create(), Times.Once());
                    }
                }
            }

            public class AndInvokingClean1 : AndInvokingRun
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    PHmiRunTarget.Clean();
                }

                public class ThenClientIsDisposed : AndInvokingClean1
                {
                    [Test]
                    public void Test()
                    {
                        Client.Verify(c => c.Dispose(), Times.Once());
                    }
                }

                public class AndInvokingRunAfterClean : AndInvokingClean1
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        PHmiRunTarget.Run();
                    }

                    public class ThenClientIsCreatedAgain : AndInvokingRunAfterClean
                    {
                        [Test]
                        public void Test()
                        {
                            ClientFactory.Verify(f => f.Create(), Times.Exactly(2));
                        }
                    }
                }
            }
            
            public class AndClientDisposeThrows : AndInvokingRun
            {
                public Exception ClientDisposeException;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    ClientDisposeException = new Exception("Client dispose exc");
                    Client.Setup(c => c.Dispose()).Throws(ClientDisposeException);
                }

                public class AndInvokingClean2 : AndClientDisposeThrows
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        PHmiRunTarget.Clean();
                    }

                    public class ThenExceptionIsReported : AndInvokingClean2
                    {
                        [Test]
                        public void Test()
                        {
                            Reporter.Verify(r => r.Report(string.Format("{0}: {1}", PHmiRunTarget.Name, Res.CleanError),
                                ClientDisposeException));
                        }
                    }
                }
            }

            public class ThenTargetsRun : AndInvokingRun
            {
                [Test]
                public void Test()
                {
                    Target1.Verify(t => t.Run(Client.Object), Times.Once());
                    Target2.Verify(t => t.Run(Client.Object), Times.Once());
                }
            }
        }

        public class AndInvokingClean : WhenUsingPHmiRunTarget
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                PHmiRunTarget.Clean();
            }

            public class ThenNothingHappens : AndInvokingClean
            {
                [Test]
                public void Test()
                {
                }
            }
        }

        public class AndTargetRunThrows : WhenUsingPHmiRunTarget
        {
            protected Exception Exception;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Exception = new Exception("Exception");
                Target1.Setup(t => t.Run(Client.Object)).Throws(Exception);
            }

            public class AndInvokingRun1 : AndTargetRunThrows
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    PHmiRunTarget.Run();
                }

                public class ThenTargetIsCleaned : AndInvokingRun1
                {
                    [Test]
                    public void Test()
                    {
                        Target1.Verify(t => t.Clean(), Times.Once());
                    }
                }

                public class ThenExceptionIsReported : AndInvokingRun1
                {
                    [Test]
                    public void Test()
                    {
                        Reporter.Verify(r => r.Report(Target1.Object.Name + ": " + Res.RunError, Exception), Times.Once());
                    }
                }

                public class ThenClientIsDisposed : AndInvokingRun1
                {
                    [Test]
                    public void Test()
                    {
                        Client.Verify(c => c.Dispose(), Times.Once());
                    }
                }

                public class ThenClientIsCreatedAgain : AndInvokingRun1
                {
                    [Test]
                    public void Test()
                    {
                        ClientFactory.Verify(f => f.Create(), Times.Exactly(2));
                    }
                }

                public class ThenOtherTargetsRun : AndInvokingRun1
                {
                    [Test]
                    public void Test()
                    {
                        Target2.Verify(t => t.Run(Client.Object), Times.Once());
                    }
                }
            }
            
            public class AndTargetCleanThrows : AndTargetRunThrows
            {
                protected Exception CleanException;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    CleanException = new Exception("CleanException");
                    Target1.Setup(t => t.Clean()).Throws(CleanException);
                }

                public class AndInvokingRun2 : AndTargetCleanThrows
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        PHmiRunTarget.Run();
                    }

                    public class ThenExceptionIsReported : AndInvokingRun2
                    {
                        [Test]
                        public void Test()
                        {
                            Reporter.Verify(r => r.Report(Target1.Object.Name + ": " + Res.CleanError, CleanException), Times.Once());
                        }
                    }
                }
            }
        }
    }
}
