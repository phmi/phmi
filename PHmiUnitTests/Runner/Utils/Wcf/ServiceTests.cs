using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using PHmiClient.Alarms;
using PHmiClient.Users;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Pagination;
using PHmiClient.Wcf;
using PHmiClient.Wcf.ServiceTypes;
using PHmiClientUnitTests;
using PHmiRunner.Utils;
using PHmiRunner.Utils.Alarms;
using PHmiRunner.Utils.IoDeviceRunner;
using PHmiRunner.Utils.Users;
using PHmiRunner.Utils.Wcf;

namespace PHmiUnitTests.Runner.Utils.Wcf
{
    public class WhenWorkingWithService : Specification
    {
        internal IService Service;
        protected Mock<IProject> Project;
        protected Mock<ITimeService> TimeService;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            Project = new Mock<IProject>();
            TimeService = new Mock<ITimeService>();
            Service = new Service(Project.Object, null, TimeService.Object);
        }

        public class AndProjectContainsIoDeviceRunTarget : WhenWorkingWithService
        {
            protected Mock<IDictionary<int, IIoDeviceRunTarget>> IoDeviceRunTargets;
            protected Mock<IIoDeviceRunTarget> IoDeviceRunTarget;
            protected const int IoDeviceId = 10;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                IoDeviceRunTargets = new Mock<IDictionary<int, IIoDeviceRunTarget>>();
                Project.Setup(p => p.IoDeviceRunTargets).Returns(IoDeviceRunTargets.Object);
                IoDeviceRunTarget = new Mock<IIoDeviceRunTarget>();
                IoDeviceRunTargets.SetupGet(t => t[IoDeviceId]).Returns(IoDeviceRunTarget.Object);
            }

            public class AndRemapTagsInvoked : AndProjectContainsIoDeviceRunTarget
            {
                internal RemapTagsParameter RemapTagsParameter;
                internal RemapTagsResult[] RemapTagsResult;
                protected const int DigReadId = 10;
                protected const bool DigReadValue = true;
                protected const int NumReadId = 11;
                protected const double NumReadValue = 234.4312;
                protected Mock<INotificationReporter> Reporter;
                protected ReadOnlyObservableCollection<Notification> Notifications; 

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    RemapTagsParameter = new RemapTagsParameter
                        {
                            IoDeviceId = IoDeviceId,
                            DigWriteIds = new [] {20},
                            DigWriteValues = new [] {true},
                            NumWriteIds = new [] {30},
                            NumWriteValues = new [] {33.3},
                            DigReadIds = new [] {DigReadId},
                            NumReadIds = new [] {NumReadId}
                        };
                    IoDeviceRunTarget.Setup(t => t.GetDigitalValue(DigReadId)).Returns(DigReadValue).Verifiable();
                    IoDeviceRunTarget.Setup(t => t.GetNumericValue(NumReadId)).Returns(NumReadValue).Verifiable();

                    Notifications = new ReadOnlyObservableCollection<Notification>(
                        new ObservableCollection<Notification>{new Notification(DateTime.Now, "Message", "ShortDesc", "LongDesc")});
                    Reporter = new Mock<INotificationReporter>();
                    Reporter.SetupGet(r => r.Notifications).Returns(Notifications);
                    IoDeviceRunTarget.SetupGet(t => t.Reporter).Returns(Reporter.Object);

                    RemapTagsResult = Service.RemapTags(new [] { RemapTagsParameter });
                }

                public class ThenRemapPerformed : AndRemapTagsInvoked
                {
                    [Test]
                    public void SetDigTagsPerformed()
                    {
                        IoDeviceRunTarget.Verify(t => t.SetDigitalValue(RemapTagsParameter.DigWriteIds[0], RemapTagsParameter.DigWriteValues[0]), Times.Once());
                    }

                    [Test]
                    public void SetNumTagsPerformed()
                    {
                        IoDeviceRunTarget.Verify(t => t.SetNumericValue(RemapTagsParameter.NumWriteIds[0], RemapTagsParameter.NumWriteValues[0]), Times.Once());
                    }

                    [Test]
                    public void GetTagsPerformed()
                    {
                        IoDeviceRunTarget.Verify();
                    }

                    [Test]
                    public void DigTagsValuesReturned()
                    {
                        Assert.That(RemapTagsResult[0].DigReadValues[0], Is.EqualTo(DigReadValue));
                    }

                    [Test]
                    public void NumTagsValuesReturned()
                    {
                        Assert.That(RemapTagsResult[0].NumReadValues[0], Is.EqualTo(NumReadValue));
                    }
                }

                public class ThenNotificationsPassedToResult : AndRemapTagsInvoked
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(RemapTagsResult[0].Notifications.Length, Is.EqualTo(Notifications.Count));
                        Assert.That(RemapTagsResult[0].Notifications[0].StartTime, Is.EqualTo(Notifications[0].StartTime));
                        Assert.That(RemapTagsResult[0].Notifications[0].Message, Is.EqualTo(Notifications[0].Message));
                        Assert.That(RemapTagsResult[0].Notifications[0].ShortDescription, Is.EqualTo(Notifications[0].ShortDescription));
                        Assert.That(RemapTagsResult[0].Notifications[0].LongDescription, Is.EqualTo(Notifications[0].LongDescription));
                    }
                }

                public class ThenWriteLockIsTaken : AndRemapTagsInvoked
                {
                    [Test]
                    public void Test()
                    {
                        IoDeviceRunTarget.Verify(t => t.EnterWriteLock(), Times.Once());
                        IoDeviceRunTarget.Verify(t => t.ExitWriteLock(), Times.Once());
                        IoDeviceRunTarget.Verify(t => t.EnterReadLock(), Times.Never());
                        IoDeviceRunTarget.Verify(t => t.ExitReadLock(), Times.Never());
                    }
                }
            }

            public class AndReadOnlyRemapTagsInvoked : AndProjectContainsIoDeviceRunTarget
            {
                internal RemapTagsParameter RemapTagsParameter;
                internal RemapTagsResult[] RemapTagsResult;
                protected const int DigReadId = 10;
                protected const bool DigReadValue = true;
                protected const int NumReadId = 11;
                protected const double NumReadValue = 234.4312;
                protected Mock<INotificationReporter> Reporter;
                protected ReadOnlyObservableCollection<Notification> Notifications;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    RemapTagsParameter = new RemapTagsParameter
                    {
                        IoDeviceId = IoDeviceId,
                        DigWriteIds = new int[0],
                        DigWriteValues = new bool[0],
                        NumWriteIds = new int[0],
                        NumWriteValues = new double[0],
                        DigReadIds = new[] { DigReadId },
                        NumReadIds = new[] { NumReadId }
                    };
                    IoDeviceRunTarget.Setup(t => t.GetDigitalValue(DigReadId)).Returns(DigReadValue).Verifiable();
                    IoDeviceRunTarget.Setup(t => t.GetNumericValue(NumReadId)).Returns(NumReadValue).Verifiable();

                    Notifications = new ReadOnlyObservableCollection<Notification>(
                        new ObservableCollection<Notification> { new Notification(DateTime.Now, "Message", "ShortDesc", "LongDesc") });
                    Reporter = new Mock<INotificationReporter>();
                    Reporter.SetupGet(r => r.Notifications).Returns(Notifications);
                    IoDeviceRunTarget.SetupGet(t => t.Reporter).Returns(Reporter.Object);

                    RemapTagsResult = Service.RemapTags(new[] { RemapTagsParameter });
                }

                public class ThenRemapPerformed : AndReadOnlyRemapTagsInvoked
                {
                    [Test]
                    public void SetDigTagsNotPerformed()
                    {
                        IoDeviceRunTarget.Verify(t => t.SetDigitalValue(It.IsAny<int>(), It.IsAny<bool>()), Times.Never());
                    }

                    [Test]
                    public void SetNumTagsNotPerformed()
                    {
                        IoDeviceRunTarget.Verify(t => t.SetNumericValue(It.IsAny<int>(), It.IsAny<double>()), Times.Never());
                    }

                    [Test]
                    public void GetTagsPerformed()
                    {
                        IoDeviceRunTarget.Verify();
                    }

                    [Test]
                    public void DigTagsValuesReturned()
                    {
                        Assert.That(RemapTagsResult[0].DigReadValues[0], Is.EqualTo(DigReadValue));
                    }

                    [Test]
                    public void NumTagsValuesReturned()
                    {
                        Assert.That(RemapTagsResult[0].NumReadValues[0], Is.EqualTo(NumReadValue));
                    }
                }

                public class ThenNotificationsPassedToResult : AndReadOnlyRemapTagsInvoked
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(RemapTagsResult[0].Notifications.Length, Is.EqualTo(Notifications.Count));
                        Assert.That(RemapTagsResult[0].Notifications[0].StartTime, Is.EqualTo(Notifications[0].StartTime));
                        Assert.That(RemapTagsResult[0].Notifications[0].Message, Is.EqualTo(Notifications[0].Message));
                        Assert.That(RemapTagsResult[0].Notifications[0].ShortDescription, Is.EqualTo(Notifications[0].ShortDescription));
                        Assert.That(RemapTagsResult[0].Notifications[0].LongDescription, Is.EqualTo(Notifications[0].LongDescription));
                    }
                }

                public class ThenReadLockIsTaken : AndReadOnlyRemapTagsInvoked
                {
                    [Test]
                    public void Test()
                    {
                        IoDeviceRunTarget.Verify(t => t.EnterWriteLock(), Times.Never());
                        IoDeviceRunTarget.Verify(t => t.ExitWriteLock(), Times.Never());
                        IoDeviceRunTarget.Verify(t => t.EnterReadLock(), Times.Once());
                        IoDeviceRunTarget.Verify(t => t.ExitReadLock(), Times.Once());
                    }
                }
            }
        }

        public class AndProjectContainsUsersRunner : WhenWorkingWithService
        {
            protected Mock<IUsersRunner> UsersRunner;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                UsersRunner = new Mock<IUsersRunner>();
                Project.Setup(p => p.UsersRunner).Returns(UsersRunner.Object);
            }

            public class ThenLogOnReturnsUsersRunnerLogOn : AndProjectContainsUsersRunner
            {
                protected string Name;
                protected string Password;
                protected User User;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Name = "Name";
                    Password = "Password";
                    User = new User();
                    UsersRunner.Setup(r => r.LogOn(Name, Password)).Returns(User);
                }

                [Test]
                public void Test()
                {
                    Assert.That(Service.LogOn(Name, Password), Is.SameAs(User));
                }
            }

            public class ThenChangePasswordReturnsUsersChangePassword : AndProjectContainsUsersRunner
            {
                protected string Name;
                protected string Password;
                protected string NewPassword;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Name = "Name";
                    Password = "Password";
                    NewPassword = "NewPassword";
                    UsersRunner.Setup(r => r.ChangePassword(Name, Password, NewPassword)).Returns(true);
                }

                [Test]
                public void Test()
                {
                    Assert.That(Service.ChangePassword(Name, Password, NewPassword), Is.EqualTo(true));
                    UsersRunner.Verify(r => r.ChangePassword(Name, Password, NewPassword), Times.Once());
                }
            }
        }

        public class AndTimeServiceReturnsTime : WhenWorkingWithService
        {
            protected DateTime DateTime;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                DateTime = DateTime.UtcNow;
                TimeService.Setup(s => s.UtcTime).Returns(DateTime);
            } 
        }

        public class ThenUpdateStatusReturnsTime : AndTimeServiceReturnsTime
        {
            [Test]
            public void Test()
            {
                Assert.That(Service.UpdateStatus().Time, Is.EqualTo(DateTime));
            }
        }

        public class AndProjectContainsAlarmsRunTarget : WhenWorkingWithService
        {
            protected int Id;
            protected Mock<IAlarmsRunTarget> AlarmsRunTarget;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                AlarmsRunTarget = new Mock<IAlarmsRunTarget>();
                var dic = new Dictionary<int, IAlarmsRunTarget>
                              {
                                  {Id, AlarmsRunTarget.Object}
                              };
                Project.Setup(p => p.AlarmsRunTargets).Returns(dic);
            }

            public class AndRemapAlarmsInvoked : AndProjectContainsAlarmsRunTarget
            {
                internal RemapAlarmsParameter Parameter;
                internal Alarm[] Current;
                internal Alarm[] History;
                internal RemapAlarmResult Result;
                internal Notification Notification;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Parameter = new RemapAlarmsParameter
                        {
                            AcknowledgeParameters = new Tuple<AlarmSampleId[], Identity>[0],
                            GetStatus = true,
                            CategoryId = Id,
                            CurrentParameters = new []
                                {
                                    new Tuple<CriteriaType, AlarmSampleId, int>(
                                        CriteriaType.DownFromInfinity, new AlarmSampleId(
                                            DateTime.Now, RandomGenerator.GetRandomInt32()), RandomGenerator.GetRandomInt32())
                                },
                            HistoryParameters = new[]
                                {
                                    new Tuple<CriteriaType, AlarmSampleId, int>(
                                        CriteriaType.DownFromInfinity, new AlarmSampleId(
                                            DateTime.Now, RandomGenerator.GetRandomInt32()), RandomGenerator.GetRandomInt32())
                                },
                        };
                    AlarmsRunTarget.Setup(t => t.GetHasActiveAndUnacknowledged()).Returns(new Tuple<bool, bool>(true, false)).Verifiable();
                    var curPar = Parameter.CurrentParameters.Single();
                    Current = new Alarm[0];
                    AlarmsRunTarget.Setup(t => t.GetCurrentAlarms(curPar.Item1, curPar.Item2, curPar.Item3)).Returns(Current).Verifiable();
                    var histPar = Parameter.HistoryParameters.Single();
                    History = new Alarm[0];
                    AlarmsRunTarget.Setup(t => t.GetHistoryAlarms(histPar.Item1, histPar.Item2, histPar.Item3)).Returns(History).Verifiable();
                    Notification = new Notification(DateTime.Now, "Message", "Short", "Long");
                    AlarmsRunTarget.Setup(t => t.Reporter.Notifications).Returns(
                        new ReadOnlyObservableCollection<Notification>(new ObservableCollection<Notification>
                            {
                                Notification
                            })).Verifiable();
                    Result = Service.RemapAlarms(new [] {Parameter}).Single();
                }

                public class ThenRunTargetUsed : AndRemapAlarmsInvoked
                {
                    [Test]
                    public void Test()
                    {
                        AlarmsRunTarget.Verify();
                    }
                }

                public class ThenGetHasActiveInvoked : AndRemapAlarmsInvoked
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Result.HasActive, Is.True);
                    }
                }

                public class ThenGetHasUnacknowledgedInvoked : AndRemapAlarmsInvoked
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Result.HasUnacknowledged, Is.False);
                    }
                }

                public class ThenGetCurParametersIsRead : AndRemapAlarmsInvoked
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Result.Current.Single(), Is.SameAs(Current));
                    }
                }

                public class ThenGetHistoryParametersIsRead : AndRemapAlarmsInvoked
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Result.History.Single(), Is.SameAs(History));
                    }
                }

                public class ThenReporterNotificationsAreRead : AndRemapAlarmsInvoked
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Result.Notifications.Single().StartTime == Notification.StartTime);
                        Assert.That(Result.Notifications.Single().Message == Notification.Message);
                        Assert.That(Result.Notifications.Single().ShortDescription == Notification.ShortDescription);
                        Assert.That(Result.Notifications.Single().LongDescription == Notification.LongDescription);
                    }
                }
            }

            public class AndRemapAlarmsInvokedEmpty : AndProjectContainsAlarmsRunTarget
            {
                internal RemapAlarmsParameter Parameter;
                internal RemapAlarmResult Result;
                internal Notification Notification;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Parameter = new RemapAlarmsParameter
                    {
                        AcknowledgeParameters = new Tuple<AlarmSampleId[], Identity>[0],
                        GetStatus = false,
                        CategoryId = Id,
                        CurrentParameters = new Tuple<CriteriaType, AlarmSampleId, int>[0],
                        HistoryParameters = new Tuple<CriteriaType, AlarmSampleId, int>[0]
                    };
                    Notification = new Notification(DateTime.Now, "Message", "Short", "Long");
                    AlarmsRunTarget.Setup(t => t.Reporter.Notifications).Returns(
                        new ReadOnlyObservableCollection<Notification>(new ObservableCollection<Notification>
                            {
                                Notification
                            })).Verifiable();
                    Result = Service.RemapAlarms(new[] { Parameter }).Single();
                }

                public class ThenGetHasActiveAndUnacknowledgedNotInvoked : AndRemapAlarmsInvokedEmpty
                {
                    [Test]
                    public void Test()
                    {
                        AlarmsRunTarget.Verify(t => t.GetHasActiveAndUnacknowledged(), Times.Never());
                    }
                }

                public class ThenGetCurParametersIsNotRead : AndRemapAlarmsInvokedEmpty
                {
                    [Test]
                    public void Test()
                    {
                        AlarmsRunTarget.Verify(t => t.GetCurrentAlarms(
                            It.IsAny<CriteriaType>(), It.IsAny<AlarmSampleId>(), It.IsAny<int>()),
                            Times.Never());
                    }
                }

                public class ThenGetHistoryParametersIsNotRead : AndRemapAlarmsInvokedEmpty
                {
                    [Test]
                    public void Test()
                    {
                        AlarmsRunTarget.Verify(t => t.GetCurrentAlarms(
                            It.IsAny<CriteriaType>(), It.IsAny<AlarmSampleId>(), It.IsAny<int>()),
                            Times.Never());
                    }
                }

                public class ThenReporterNotificationsAreRead : AndRemapAlarmsInvokedEmpty
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Result.Notifications.Single().StartTime == Notification.StartTime);
                        Assert.That(Result.Notifications.Single().Message == Notification.Message);
                        Assert.That(Result.Notifications.Single().ShortDescription == Notification.ShortDescription);
                        Assert.That(Result.Notifications.Single().LongDescription == Notification.LongDescription);
                    }
                }
            }
        }
    }
}
