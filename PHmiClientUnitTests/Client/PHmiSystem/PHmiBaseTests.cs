using System;
using Moq;
using NUnit.Framework;
using PHmiClient.Alarms;
using PHmiClient.Logs;
using PHmiClient.PHmiSystem;
using PHmiClient.Tags;
using PHmiClient.Trends;
using PHmiClient.Users;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Runner;
using PHmiClient.Wcf;

namespace PHmiClientUnitTests.Client.PHmiSystem
{
    public class WhenUsingPHmiBase : Specification
    {
        protected Mock<INotificationReporterFactory> ReporterFactory;
        protected Mock<INotificationReporter> Reporter;
        internal Mock<IServiceClientFactory> ServiceClientFactory;
        protected Mock<ICyclicRunnerFactory> RunnerFactory;
        protected Mock<ICyclicRunner> Runner;
        internal Mock<IUsersRunTarget> UsersRunTarget;
        internal Mock<ITagServiceFactory> TagServiceFactory;
        internal Mock<ITagService> TagService;
        internal Mock<IAlarmServiceFactory> AlarmServiceFactory;
        internal Mock<IAlarmService> AlarmService;
        internal Mock<ITrendsServiceFactory> TrendsServiceFactory;
        internal Mock<ITrendsService> TrendsService;
        internal Mock<ILogService> LogService;
        internal Mock<IPHmiRunTargetFactory> PHmiRunTargetFactory;
        internal Mock<IPHmiRunTarget> PHmiRunTarget;
        internal Mock<IEventRunTarget> BeforeUpdateRunTarget;
        internal Mock<IEventRunTarget> AfterUpdateRunTarget;
        protected Mock<ITimeService> TimeService;
        protected Mock<ITimerService> TimerService;
        internal Mock<IUpdateStatusRunTargetFactory> UpdateStatusRunTargetFactory;
        internal Mock<IUpdateStatusRunTarget> UpdateStatusRunTarget;
        protected PHmiAbstract PHmi;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            ReporterFactory = new Mock<INotificationReporterFactory>();
            ServiceClientFactory = new Mock<IServiceClientFactory>();
            RunnerFactory = new Mock<ICyclicRunnerFactory>();
            UsersRunTarget = new Mock<IUsersRunTarget>();
            TagServiceFactory = new Mock<ITagServiceFactory>();
            PHmiRunTargetFactory = new Mock<IPHmiRunTargetFactory>();
            BeforeUpdateRunTarget = new Mock<IEventRunTarget>();
            AfterUpdateRunTarget = new Mock<IEventRunTarget>();
            TimeService = new Mock<ITimeService>();
            TimerService = new Mock<ITimerService>();
            UpdateStatusRunTargetFactory = new Mock<IUpdateStatusRunTargetFactory>();
            UpdateStatusRunTarget = new Mock<IUpdateStatusRunTarget>();
            UpdateStatusRunTargetFactory.Setup(f => f.Create(TimeService.Object)).Returns(UpdateStatusRunTarget.Object);

            Reporter = new Mock<INotificationReporter>();
            ReporterFactory.Setup(f => f.Create(TimeService.Object))
                .Returns(Reporter.Object);
            TagService = new Mock<ITagService>();
            TagServiceFactory.Setup(f => f.Create(Reporter.Object)).Returns(TagService.Object);
            AlarmServiceFactory = new Mock<IAlarmServiceFactory>();
            AlarmService = new Mock<IAlarmService>();
            AlarmServiceFactory.Setup(f => f.Create(Reporter.Object)).Returns(AlarmService.Object);
            TrendsServiceFactory = new Mock<ITrendsServiceFactory>();
            TrendsService = new Mock<ITrendsService>();
            TrendsServiceFactory.Setup(f => f.Create(Reporter.Object)).Returns(TrendsService.Object);
            LogService = new Mock<ILogService>();
            PHmiRunTarget = new Mock<IPHmiRunTarget>();
            PHmiRunTargetFactory.Setup(f => f.Create(
                Reporter.Object,
                ServiceClientFactory.Object,
                BeforeUpdateRunTarget.Object,
                UpdateStatusRunTarget.Object,
                UsersRunTarget.Object,
                TagService.Object,
                AlarmService.Object,
                TrendsService.Object,
                LogService.Object,
                AfterUpdateRunTarget.Object))
                .Returns(PHmiRunTarget.Object);
            Runner = new Mock<ICyclicRunner>();
            RunnerFactory.Setup(f => f.Create(PHmiRunTarget.Object)).Returns(Runner.Object);
            
            PHmi = new PHmiBase(
                ReporterFactory.Object,
                ServiceClientFactory.Object,
                RunnerFactory.Object,
                PHmiRunTargetFactory.Object,
                TimeService.Object,
                TimerService.Object,
                BeforeUpdateRunTarget.Object,
                UpdateStatusRunTargetFactory.Object,
                UsersRunTarget.Object,
                TagServiceFactory.Object,
                AlarmServiceFactory.Object,
                TrendsServiceFactory.Object,
                LogService.Object,
                AfterUpdateRunTarget.Object);
        }

        public class ThenReporterReturnsReporter : WhenUsingPHmiBase
        {
            [Test]
            public void Test()
            {
                Assert.That(PHmi.Reporter, Is.SameAs(Reporter.Object));
            }
        }

        public class AndIoDeviceAdded : WhenUsingPHmiBase
        {
            protected Mock<IoDeviceAbstract> IoDevice;
            protected IoDeviceAbstract ReturnedIoDevice;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                IoDevice = new Mock<IoDeviceAbstract>();
                ReturnedIoDevice = PHmi.AddIoDevice(IoDevice.Object);
            }

            public class ThenIoDevicesContainsIt : AndIoDeviceAdded
            {
                [Test]
                public void Test()
                {
                    Assert.That(PHmi.IoDevices, Contains.Item(IoDevice.Object));
                }
            }

            public class ThenAddedToTagService : AndIoDeviceAdded
            {
                [Test]
                public void Test()
                {
                    TagService.Verify(t => t.Add(IoDevice.Object), Times.Once());
                }
            }

            public class ThenItIsReturned : AndIoDeviceAdded
            {
                [Test]
                public void Test()
                {
                    Assert.That(ReturnedIoDevice, Is.SameAs(IoDevice.Object));
                }
            }
        }

        public class AndInvokingStart : WhenUsingPHmiBase
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                PHmi.Start();
            }

            public class ThenRunnerStarted : AndInvokingStart
            {
                [Test]
                public void Test()
                {
                    Runner.Verify(r => r.Start(), Times.Once());
                }
            }

            public class ThenTimerStarted : AndInvokingStart
            {
                [Test]
                public void Test()
                {
                    TimerService.Verify(t => t.Start(), Times.Once());
                }
            }
        }

        public class AndInvokingStop : WhenUsingPHmiBase
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                PHmi.Stop();
            }

            public class ThenRunnerStarted : AndInvokingStop
            {
                [Test]
                public void Test()
                {
                    Runner.Verify(r => r.Stop(), Times.Once());
                }
            }

            public class ThenTimerStarted : AndInvokingStop
            {
                [Test]
                public void Test()
                {
                    TimerService.Verify(t => t.Stop(), Times.Once());
                }
            }
        }

        public class AndRegisteringBeforeUpdate : WhenUsingPHmiBase
        {
            protected int Count;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Count = 0;
                PHmi.BeforeUpdate += (sender, args) =>
                {
                    Count++;
                };
            }

            public class AndRunned : AndRegisteringBeforeUpdate
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    BeforeUpdateRunTarget.Raise(t => t.Runned += null, EventArgs.Empty);
                }

                public class ThenBeforeUpdateRaised : AndRunned
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Count, Is.EqualTo(1));
                    }
                }
            }
        }

        public class AndRegisteringAfterUpdate : WhenUsingPHmiBase
        {
            protected int Count;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Count = 0;
                PHmi.AfterUpdate += (sender, args) =>
                    {
                        Count++;
                    };
            }

            public class AndRunned : AndRegisteringAfterUpdate
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    AfterUpdateRunTarget.Raise(t => t.Runned += null, EventArgs.Empty);
                }

                public class ThenAfterUpdateRaised : AndRunned
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Count, Is.EqualTo(1));
                    }
                }
            }
        }

        public class AndRegisteringPropertyChangedForTime : WhenUsingPHmiBase
        {
            protected int Count;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Count = 0;
                PHmi.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "Time")
                        Count++;
                };
            }

            public class AndTimerAlapsed : AndRegisteringPropertyChangedForTime
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    TimerService.Raise(t => t.Elapsed += null, EventArgs.Empty);
                }

                public class ThenTimeChangedRaised : AndTimerAlapsed
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Count, Is.EqualTo(1));
                    }
                }
            }
        }

        public class ThenTimeReturnsUtcTime : WhenUsingPHmiBase
        {
            protected DateTime CurrentTime;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                CurrentTime = DateTime.UtcNow;
                TimeService.Setup(s => s.UtcTime).Returns(CurrentTime);
            }

            [Test]
            public void Test()
            {
                Assert.That(PHmi.Time, Is.EqualTo(CurrentTime));
            }
        }
    }
}
