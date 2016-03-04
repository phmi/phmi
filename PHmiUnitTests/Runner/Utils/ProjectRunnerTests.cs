using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Runner;
using PHmiClientUnitTests;
using PHmiModel;
using PHmiModel.Entities;
using PHmiModel.Interfaces;
using PHmiResources.Loc;
using PHmiRunner.Utils;
using PHmiRunner.Utils.Alarms;
using PHmiRunner.Utils.IoDeviceRunner;
using PHmiRunner.Utils.Logs;
using PHmiRunner.Utils.Trends;
using PHmiRunner.Utils.Users;
using PHmiRunner.Utils.Wcf;

namespace PHmiUnitTests.Runner.Utils
{
    public class WhenUsingProjectRunner : Specification
    {
        protected IProjectRunner ProjectRunner;
        protected Mock<IModelContext> Context;
        protected Mock<ITimeService> TimeService;
        protected Mock<IReporter> Reporter;
        protected Mock<IServiceRunnerFactory> ServiceRunnerFactory;
        protected Mock<IRunner> ServiceRunner;
        protected Mock<ICyclicRunnerFactory> CyclicRunnerFactory;
        protected Mock<IDataDbCreatorFactory> DataDbCreatorFactory;
        protected Mock<IIoDeviceRunTargetFactory> IoDeviceRunTargetFactory;
        protected Mock<IUsersRunner> UsersRunner;
        protected Mock<IUsersRunnerFactory> UsersRunnerFactory;
        protected Mock<IDataDbCreator> DataDbCreator;
        protected Mock<IAlarmsRunTargetFactory> AlarmsRunTargetFactory;
        protected Mock<ITrendsRunTargetFactory> TrendsRunTargetFactory;
        protected Mock<ILogRunTargetFactory> LogRunTargetFactory;
        protected const string ProjectName = "Project";
        protected const string Server = "Server";
        protected Settings Settings;
        protected const string DataDbConStr = "DataDbConStr";

        protected override void EstablishContext()
        {
            base.EstablishContext();
            Context = new Mock<IModelContext>();
            TimeService = new Mock<ITimeService>();
            Reporter = new Mock<IReporter>();
            ServiceRunnerFactory = new Mock<IServiceRunnerFactory>();
            CyclicRunnerFactory = new Mock<ICyclicRunnerFactory>();
            IoDeviceRunTargetFactory = new Mock<IIoDeviceRunTargetFactory>();
            DataDbCreatorFactory = new Mock<IDataDbCreatorFactory>();
            DataDbCreator = new Mock<IDataDbCreator>();
            DataDbCreatorFactory.Setup(f => f.Create(DataDbConStr, Reporter.Object)).Returns(DataDbCreator.Object);
            UsersRunner = new Mock<IUsersRunner>();
            UsersRunnerFactory = new Mock<IUsersRunnerFactory>();
            UsersRunnerFactory.Setup(f => f.Create(Context.Object, DataDbConStr)).Returns(
                UsersRunner.Object);
            AlarmsRunTargetFactory = new Mock<IAlarmsRunTargetFactory>();
            TrendsRunTargetFactory = new Mock<ITrendsRunTargetFactory>();
            LogRunTargetFactory = new Mock<ILogRunTargetFactory>();
            ProjectRunner = new ProjectRunner(
                ProjectName,
                Context.Object,
                TimeService.Object,
                Reporter.Object,
                DataDbConStr,
                DataDbCreatorFactory.Object,
                UsersRunnerFactory.Object,
                ServiceRunnerFactory.Object,
                CyclicRunnerFactory.Object,
                IoDeviceRunTargetFactory.Object,
                AlarmsRunTargetFactory.Object,
                TrendsRunTargetFactory.Object,
                LogRunTargetFactory.Object);
            ServiceRunner = new Mock<IRunner>();
            ServiceRunnerFactory.Setup(f => f.Create(ProjectRunner, Server, null, TimeService.Object)).Returns(ServiceRunner.Object);
            Settings = new Settings
            {
                Server = Server
            };
            Context.Setup(c => c.Get<Settings>()).Returns(new EnumerableQuery<Settings>(new[] { Settings }));
        }

        public class AndContextContainsIoDevices : WhenUsingProjectRunner
        {
            protected PHmiModel.Entities.IoDevice IoDevice;
            protected IQueryable<PHmiModel.Entities.IoDevice> IoDevices;
            protected Mock<IIoDeviceRunTarget> IoDeviceRunTarget;
            protected Mock<ICyclicRunner> CyclicRunner;
            
            protected override void EstablishContext()
            {
                base.EstablishContext();
                IoDevice = new PHmiModel.Entities.IoDevice
                    {
                        Name = "IoDevice",
                        Id = 1
                    };
                IoDevices = new EnumerableQuery<PHmiModel.Entities.IoDevice>(new [] {IoDevice});
                Context.Setup(context => context.Get<PHmiModel.Entities.IoDevice>()).Returns(IoDevices);

                IoDeviceRunTarget = new Mock<IIoDeviceRunTarget>();
                IoDeviceRunTargetFactory.Setup(f => f.Create(TimeService.Object, IoDevice))
                    .Returns(IoDeviceRunTarget.Object);
                CyclicRunner = new Mock<ICyclicRunner>();
                CyclicRunnerFactory.Setup(f => f.Create(IoDeviceRunTarget.Object)).Returns(CyclicRunner.Object);
            }

            public class AndStarted : AndContextContainsIoDevices
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    ProjectRunner.Start();
                }

                public class ThenIoDeviceRunTargetCreated : AndStarted
                {
                    [Test]
                    public void Test()
                    {
                        IoDeviceRunTargetFactory.Verify(f => f.Create(TimeService.Object, IoDevice), Times.Once());
                    }
                }

                public class ThenCyclicRunnerCreated : AndStarted
                {
                    [Test]
                    public void Test()
                    {
                        CyclicRunnerFactory.Verify(f => f.Create(IoDeviceRunTarget.Object), Times.Once());
                    }
                }

                public class ThenCyclicRunnerStarted : AndStarted
                {
                    [Test]
                    public void Test()
                    {
                        CyclicRunner.Verify(r => r.Start(), Times.Once());
                    }
                }

                public class ThenIoDeviceRunTargetsContainsIoDevicrRunTarget : AndStarted
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(ProjectRunner.IoDeviceRunTargets[IoDevice.Id], Is.SameAs(IoDeviceRunTarget.Object));
                    }
                }

                public class AndStopped : AndStarted
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        ProjectRunner.Stop();
                    }

                    public class ThenCyclicRunnerStopped : AndStopped
                    {
                        [Test]
                        public void Test()
                        {
                            CyclicRunner.Verify(r => r.Stop(), Times.Once());
                            Reporter.Verify(r => r.Report(string.Format(Res.IoDeviceStoppedMessage, IoDevice.Name), null, null), Times.Once());
                        }
                    }

                    public class ThenIoDeviceRunTargetIsDisposed : AndStopped
                    {
                        [Test]
                        public void Test()
                        {
                            IoDeviceRunTarget.Verify(t => t.Dispose(), Times.Once());
                        }
                    }

                    public class ThenProjectStoppedIsReported : AndStopped
                    {
                        [Test]
                        public void Test()
                        {
                            Reporter.Verify(r => r.Report(string.Format(Res.ProjectStoppingMessage, ProjectName), null, null), Times.Once());
                        }
                    }
                }

                public class AndCyclicRunnerStopThrows : AndStarted
                {
                    protected Exception CyclicRunnerException;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        CyclicRunnerException = new Exception("CyclicRunnerException");
                        CyclicRunner.Setup(r => r.Stop()).Throws(CyclicRunnerException);
                    }

                    public class AndStopExecuted : AndCyclicRunnerStopThrows
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            ProjectRunner.Stop();
                        }

                        public class ThenCyclicRunnerExceptionIsReported : AndStopExecuted
                        {
                            [Test]
                            public void Test()
                            {
                                Reporter.Verify(r => r.Report(It.IsAny<string>(), CyclicRunnerException));
                            }
                        }

                        public class ThenProjectStoppedIsNotReported : AndStopExecuted
                        {
                            [Test]
                            public void Test()
                            {
                                Reporter.Verify(r => r.Report(string.Format(Res.ProjectStoppedMessage, ProjectName), null, null), Times.Never());
                            }
                        }
                    }
                }

                public class ThenIoDeviceStartedIsReported : AndStarted
                {
                    [Test]
                    public void Test()
                    {
                        Reporter.Verify(r => r.Report(string.Format(Res.IoDeviceStartedMessage, IoDevice.Name), null, null), Times.Once());
                    }
                }

                public class ThenProjectStartedIsReported : AndStarted
                {
                    [Test]
                    public void Test()
                    {
                        Reporter.Verify(r => r.Report(string.Format(Res.ProjectStartedMessage, ProjectName), null, null), Times.Once());
                    }
                }
            }

            public class AndRunnerStartThrows : AndContextContainsIoDevices
            {
                protected Exception CyclicRunnerException;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    CyclicRunnerException = new Exception("CyclicRunnerException");
                    CyclicRunner.Setup(r => r.Start()).Throws(CyclicRunnerException);
                }

                public class AndStartExecuted : AndRunnerStartThrows
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        ProjectRunner.Start();
                    }

                    public class ThenCyclicRunnerExceptionIsReported : AndStartExecuted
                    {
                        [Test]
                        public void Test()
                        {
                            Reporter.Verify(r => r.Report(It.IsAny<string>(), CyclicRunnerException), Times.Once());
                        }
                    }

                    public class ThenProjectStartedIsNotReported : AndStartExecuted
                    {
                        [Test]
                        public void Test()
                        {
                            Reporter.Verify(r => r.Report(string.Format(Res.ProjectStartedMessage, ProjectName), null, null), Times.Never());
                        }
                    }
                }
            }
        }

        public class AndContextContainsAlarmCategories : WhenUsingProjectRunner
        {
            protected AlarmCategory AlarmCategory;
            protected IQueryable<AlarmCategory> Categories;
            protected Mock<ICyclicRunner> CyclicRunner;
            protected Mock<IAlarmsRunTarget> RunTarget;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                AlarmCategory = new AlarmCategory
                                    {
                                        Name = "AlarmCategory",
                                        Id = RandomGenerator.GetRandomInt32()
                                    };
                Categories = new EnumerableQuery<AlarmCategory>(new[] { AlarmCategory });
                Context.Setup(context => context.Get<AlarmCategory>()).Returns(Categories);

                RunTarget = new Mock<IAlarmsRunTarget>();
                AlarmsRunTargetFactory.Setup(f => f.Create(DataDbConStr, ProjectRunner, AlarmCategory, TimeService.Object))
                    .Returns(RunTarget.Object);
                CyclicRunner = new Mock<ICyclicRunner>();
                CyclicRunnerFactory.Setup(f => f.Create(RunTarget.Object)).Returns(CyclicRunner.Object);
            }

            public class AndStarted : AndContextContainsAlarmCategories
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    ProjectRunner.Start();
                }

                public class ThenAlarmsRunTargetCreated : AndStarted
                {
                    [Test]
                    public void Test()
                    {
                        AlarmsRunTargetFactory.Verify(f => f.Create(DataDbConStr, ProjectRunner, AlarmCategory, TimeService.Object), Times.Once());
                    }
                }

                public class ThenCyclicRunnerCreated : AndStarted
                {
                    [Test]
                    public void Test()
                    {
                        CyclicRunnerFactory.Verify(f => f.Create(RunTarget.Object), Times.Once());
                    }
                }

                public class ThenCyclicRunnerStarted : AndStarted
                {
                    [Test]
                    public void Test()
                    {
                        CyclicRunner.Verify(r => r.Start(), Times.Once());
                    }
                }

                public class ThenAlarmsRunTargetsContainsAlarmsRunTarget : AndStarted
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(ProjectRunner.AlarmsRunTargets[AlarmCategory.Id], Is.SameAs(RunTarget.Object));
                    }
                }

                public class AndStopped : AndStarted
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        ProjectRunner.Stop();
                    }

                    public class ThenCyclicRunnerStopped : AndStopped
                    {
                        [Test]
                        public void Test()
                        {
                            CyclicRunner.Verify(r => r.Stop(), Times.Once());
                            Reporter.Verify(r => r.Report(string.Format(Res.AlarmsStoppedMessage, AlarmCategory.Name), null, null), Times.Once());
                        }
                    }

                    public class ThenProjectStoppedIsReported : AndStopped
                    {
                        [Test]
                        public void Test()
                        {
                            Reporter.Verify(r => r.Report(string.Format(Res.ProjectStoppingMessage, ProjectName), null, null), Times.Once());
                        }
                    }
                }

                public class ThenAlarmsStartedIsReported : AndStarted
                {
                    [Test]
                    public void Test()
                    {
                        Reporter.Verify(r => r.Report(string.Format(Res.AlarmsStartedMessage, AlarmCategory.Name), null, null), Times.Once());
                    }
                }
            }
        }

        public class AndContextThrows : WhenUsingProjectRunner
        {
            protected Exception ContextException;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                ContextException = new Exception("ContextException");
                Context.Setup(context => context.Get<PHmiModel.Entities.IoDevice>()).Throws(ContextException);
            }

            public class AndStarted : AndContextThrows
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    ProjectRunner.Start();
                }

                public class ThenExceptionIsReported : AndStarted
                {
                    [Test]
                    public void Test()
                    {
                        Reporter.Verify(r => r.Report(Res.StartError, ContextException), Times.Once());
                    }
                }
            }
        }

        public class AndStarted1 : WhenUsingProjectRunner
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                ProjectRunner.Start();
            }

            public class ThenStartingProjectReported : AndStarted1
            {
                [Test]
                public void Test()
                {
                    Reporter.Verify(r => r.Report(string.Format(Res.ProjectStartingMessage, ProjectName), null, null), Times.Once());
                }
            }
             
            public class ThenDataDbCreatorStarted : AndStarted1
            {
                [Test]
                public void Test()
                {
                    DataDbCreator.Verify(c => c.Start(), Times.Once());
                }
            }

            public class ThenUsersRunnerStarted : AndStarted1
            {
                [Test]
                public void Test()
                {
                    UsersRunner.Verify(r => r.Start(), Times.Once());
                    Assert.That(ProjectRunner.UsersRunner, Is.SameAs(UsersRunner.Object));
                }
            }

            public class ThenServiceRunnerCreated : AndStarted1
            {
                [Test]
                public void Test()
                {
                    ServiceRunnerFactory.Verify(f => f.Create(ProjectRunner, Server, null, TimeService.Object), Times.Once());
                }
            }
                
            public class ThenServiceRunnerStarted : AndStarted1
            {
                [Test]
                public void Test()
                {
                    ServiceRunner.Verify(r => r.Start(), Times.Once());
                    Reporter.Verify(r => r.Report(Res.ServiceIsStarted, null, null), Times.Once());
                }
            }

            public class AndStopped : AndStarted1
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    ProjectRunner.Stop();
                }

                public class ThenServiceRunnerStopped : AndStopped
                {
                    [Test]
                    public void Test()
                    {
                        ServiceRunner.Verify(r => r.Stop(), Times.Once());
                        Reporter.Verify(r => r.Report(Res.ServiceIsStopped, null, null), Times.Once());
                    }
                }
            }
        }

        public class AndStopped1 : WhenUsingProjectRunner
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                ProjectRunner.Stop();
            }

            public class ThenProjectStoppingIsReported : AndStopped1
            {
                [Test]
                public void Test()
                {
                    Reporter.Verify(r => r.Report(string.Format(Res.ProjectStoppingMessage, ProjectName), null, null), Times.Once());
                }
            }
        }
    }
}
