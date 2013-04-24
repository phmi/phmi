using System;
using Moq;
using NUnit.Framework;
using PHmiClient.Loc;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Runner;

namespace PHmiClientUnitTests.Client.Utils.Runner
{
    public class WhenUsingCyclicRunner : Specification
    {
        protected Mock<ITimerService> TimerService;
        protected Mock<INotificationReporter> Reporter;
        protected Mock<IRunTarget> Target;
        protected ICyclicRunner Runner;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            TimerService = new Mock<ITimerService>();
            Reporter = new Mock<INotificationReporter>();
            Target = new Mock<IRunTarget>();
            Target.SetupGet(t => t.Reporter).Returns(Reporter.Object);
            Runner = new CyclicRunner(TimerService.Object, Target.Object);
            Target.SetupGet(t => t.Name).Returns("TargetName");
        }

        public class AndStarted : WhenUsingCyclicRunner
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                Runner.Start();
            }

            public class ThenTimerServiceIsStarted : AndStarted
            {
                [Test]
                public void Test()
                {
                    TimerService.Verify(s => s.Start(), Times.Once());
                }
            }

            public class AndStopped : AndStarted
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Runner.Stop();
                }

                public class ThenTimerServiceIsStopped : AndStopped
                {
                    [Test]
                    public void Test()
                    {
                        TimerService.Verify(s => s.Stop(), Times.Once());
                    }
                }
            }
        }

        public class ThenRunIsInvokedWhenTimerServiceElapsed : WhenUsingCyclicRunner
        {
            [Test]
            public void Test()
            {
                Target.Verify(t => t.Run(), Times.Never());
                TimerService.Raise(s => s.Elapsed += null, EventArgs.Empty);
                Target.Verify(t => t.Run(), Times.Once());
            }
        }

        public class AndRunThrows : WhenUsingCyclicRunner
        {
            protected Exception Exception;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Exception = new Exception("Exception");
                Target.Setup(t => t.Run()).Throws(Exception);
            }

            public class AndTimerServiceElapsed : AndRunThrows
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    TimerService.Raise(s => s.Elapsed += null, EventArgs.Empty);
                }

                public class ThenExceptionIsReported : AndTimerServiceElapsed
                {
                    [Test]
                    public void Test()
                    {
                        Reporter.Verify(
                            r => r.Report(Target.Object.Name + ": " + Res.RunError, Exception),
                            Times.Once());
                    }
                }

                public class ThenCleanInvoked : AndTimerServiceElapsed
                {
                    [Test]
                    public void Test()
                    {
                        Target.Verify(t => t.Clean(), Times.Once());
                    }
                }
            }

            public class AndCleanThrows : AndRunThrows
            {
                protected Exception CleanException;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    CleanException = new Exception("CleanException");
                    Target.Setup(t => t.Clean()).Throws(CleanException);
                }

                public class AndTimerServiceElapsed1 : AndCleanThrows
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        TimerService.Raise(s => s.Elapsed += null, EventArgs.Empty);
                    }

                    public class ThenCleanExceptionReported : AndTimerServiceElapsed1
                    {
                        [Test]
                        public void Test()
                        {
                            Reporter.Verify(
                            r => r.Report(Target.Object.Name + ": " + Res.CleanError, CleanException),
                            Times.Once());
                        }
                    }
                }
            }
        }
    }
}
