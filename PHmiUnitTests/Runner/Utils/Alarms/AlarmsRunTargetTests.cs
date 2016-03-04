using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Npgsql;
using PHmiClient.Alarms;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Pagination;
using PHmiClientUnitTests;
using PHmiModel;
using PHmiModel.Entities;
using PHmiResources.Loc;
using PHmiRunner.Utils;
using PHmiRunner.Utils.Alarms;
using PHmiRunner.Utils.IoDeviceRunner;
using PHmiTools.Utils.Npg;

namespace PHmiUnitTests.Runner.Utils.Alarms
{
    public class WhenUsingAlarmsRunTarget : Specification
    {
        protected bool Acknowledgeable = true;
        protected long? TimeToStore;
        protected AlarmCategory AlarmCatetory;
        protected Mock<INotificationReporter> Reporter;
        protected Mock<IAlarmsRepository> Repository;
        protected Mock<IProject> Project;
        protected Mock<ITimeService> TimeService;
        protected AlarmTag AlarmTag;
        protected Mock<INpgsqlConnectionFactory> ConnectionFactory;
        protected NpgsqlConnection Connection;
        protected IAlarmsRunTarget AlarmsRunTarget;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            AlarmTag = new AlarmTag
                           {
                               DigTag = new DigTag
                                              {
                                                  Id = RandomGenerator.GetRandomInt32(),
                                                  IoDevice = new PHmiModel.Entities.IoDevice
                                                                   {
                                                                       Id = RandomGenerator.GetRandomInt32()
                                                                   }
                                              },
                               Id = RandomGenerator.GetRandomInt32(),
                               Acknowledgeable = Acknowledgeable
                           };
            AlarmCatetory = new AlarmCategory
                                {
                                    Name = "AlarmCategory",
                                    TimeToStoreDb = TimeToStore
                                };
            AlarmCatetory.AlarmTags.Add(AlarmTag);
            Reporter = new Mock<INotificationReporter>();
            Repository = new Mock<IAlarmsRepository>();
            Project = new Mock<IProject>();
            TimeService = new Mock<ITimeService>();
            TimeService.Setup(t => t.UtcTime).Returns(DateTime.UtcNow);
            ConnectionFactory = new Mock<INpgsqlConnectionFactory>();
            Connection = new NpgsqlConnection();
            ConnectionFactory.Setup(f => f.Create()).Returns(Connection);
            AlarmsRunTarget = new AlarmsRunTarget(
                AlarmCatetory,
                Reporter.Object,
                Repository.Object,
                Project.Object,
                TimeService.Object,
                ConnectionFactory.Object);
        }

        public class ThenNameReturns : WhenUsingAlarmsRunTarget
        {
            [Test]
            public void Test()
            {
                Assert.That(AlarmsRunTarget.Name, Is.EqualTo(string.Format("{0} \"{1}\"", Res.Alarms, AlarmCatetory.Name)));
            }
        }

        public class ThenReporterReturns : WhenUsingAlarmsRunTarget
        {
            [Test]
            public void Test()
            {
                Assert.That(AlarmsRunTarget.Reporter, Is.SameAs(Reporter.Object));
            }
        }

        public class AndProjectContainsTags : WhenUsingAlarmsRunTarget
        {
            protected Mock<IIoDeviceRunTarget> IoDeviceRunTarget;
            protected IDictionary<int, IIoDeviceRunTarget> IoDeviceRunTargets;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                IoDeviceRunTarget = new Mock<IIoDeviceRunTarget>();
                IoDeviceRunTargets = new Dictionary<int, IIoDeviceRunTarget>
                                         {
                                             {AlarmTag.DigTag.IoDevice.Id, IoDeviceRunTarget.Object}
                                         };

                Project.Setup(p => p.IoDeviceRunTargets).Returns(IoDeviceRunTargets);
            }

            public class AndDigTagValueIsTrue : AndProjectContainsTags
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    IoDeviceRunTarget.Setup(t => t.GetDigitalValue(AlarmTag.DigTag.Id)).Returns(true);
                }

                public class AndRepositoryActiveContainsNoAlarm : AndDigTagValueIsTrue
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Repository.Setup(r => r.GetActiveIds(Connection)).Returns(new AlarmSampleId[0]);
                    }

                    public class AndInvokingRun : AndRepositoryActiveContainsNoAlarm
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            AlarmsRunTarget.Run();
                        }

                        public class ThenAlarmSaved : AndInvokingRun
                        {
                            [Test]
                            public void Test()
                            {
                                Repository.Verify(r => r.Insert(Connection, It.Is<Tuple<DateTime, int, DateTime?>[]>(
                                    tuples => tuples.Any(t => t.Item1 == TimeService.Object.UtcTime
                                        && t.Item2 == AlarmTag.Id
                                        && t.Item3 == null))),
                                    Times.Once());
                            }
                        }

                        public class ThenAlarmSavedWithAcknowledgeTimeIfAlarmIsNotAcknowledgeable : AndInvokingRun
                        {
                            protected override void EstablishContext()
                            {
                                Acknowledgeable = false;
                                base.EstablishContext();
                            }

                            [Test]
                            public void Test()
                            {
                                Repository.Verify(r => r.Insert(Connection, It.Is<Tuple<DateTime, int, DateTime?>[]>(
                                    tuples => tuples.Any(t => t.Item3 == TimeService.Object.UtcTime))),
                                    Times.Once());
                            }
                        }
                    }
                }

                public class AndRepositoryActiveContainsAlarm : AndDigTagValueIsTrue
                {
                    protected AlarmSampleId[] ActiveIds;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        ActiveIds = new[]
                                        {
                                            new AlarmSampleId(
                                                TimeService.Object.UtcTime - TimeSpan.FromDays(1.111), AlarmTag.Id),
                                            new AlarmSampleId(
                                                TimeService.Object.UtcTime - TimeSpan.FromDays(2.222), AlarmTag.Id),
                                        };
                        Repository.Setup(r => r.GetActiveIds(Connection)).Returns(ActiveIds);
                    }

                    public class AndInvokingRun : AndRepositoryActiveContainsAlarm
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            AlarmsRunTarget.Run();
                        }

                        public class ThenAlarmNoSave : AndInvokingRun
                        {
                            [Test]
                            public void Test()
                            {
                                Repository.Verify(r => r.Insert(
                                    It.IsAny<NpgsqlConnection>(), It.IsAny<Tuple<DateTime, int, DateTime?>[]>()),
                                    Times.Never());
                            }
                        }

                        public class ThenSingleAlarmStays : AndInvokingRun
                        {
                            [Test]
                            public void Test()
                            {
                                Repository.Verify(r => r.Update(Connection, It.Is<AlarmSampleId[]>(
                                    tuples => tuples.Count(t => ActiveIds.Contains(t)) == ActiveIds.Length - 1),
                                    TimeService.Object.UtcTime), Times.Once());
                            }
                        }
                    }
                }
            }

            public class AndDigTagValueIsNotTrue : AndProjectContainsTags
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    IoDeviceRunTarget.Setup(t => t.GetDigitalValue(AlarmTag.DigTag.Id)).Returns(false);
                }

                public class AndRepositoryActiveContainsAlarm : AndDigTagValueIsNotTrue
                {
                    protected AlarmSampleId ActiveId;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        ActiveId = new AlarmSampleId(TimeService.Object.UtcTime - TimeSpan.FromDays(1.111), AlarmTag.Id);
                        Repository.Setup(r => r.GetActiveIds(Connection))
                            .Returns(new[]
                                {
                                    ActiveId
                                });
                    }

                    public class AndInvokingRun : AndRepositoryActiveContainsAlarm
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            AlarmsRunTarget.Run();
                        }

                        public class ThenAlarmReseted : AndInvokingRun
                        {
                            [Test]
                            public void Test()
                            {
                                Repository.Verify(r => r.Update(
                                    Connection,
                                    It.Is<AlarmSampleId[]>(tuples => tuples.Any(t => t.Equals(ActiveId))),
                                    TimeService.Object.UtcTime), Times.Once());
                            }
                        }
                    }
                }

                public class AndRepositoryActiveContainsNoAlarm : AndDigTagValueIsNotTrue
                {
                    protected AlarmSampleId ActiveId;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Repository.Setup(r => r.GetActiveIds(Connection)).Returns(new AlarmSampleId[0]);
                    }

                    public class AndInvokingRun : AndRepositoryActiveContainsNoAlarm
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            AlarmsRunTarget.Run();
                        }

                        public class ThenNoUpdate : AndInvokingRun
                        {
                            [Test]
                            public void Test()
                            {
                                Repository.Verify(r => r.Update(
                                    Connection,
                                    It.IsAny<AlarmSampleId[]>(),
                                    It.IsAny<DateTime>()), Times.Never());
                            }
                        }
                    }
                }
            }

            public class AndRepositoryActiveContainsSomeStrangeAlarm : AndProjectContainsTags
            {
                protected AlarmSampleId StrangeAlarm;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    StrangeAlarm = new AlarmSampleId(TimeService.Object.UtcTime - TimeSpan.FromDays(1), RandomGenerator.GetRandomInt32());
                    Repository.Setup(r => r.GetActiveIds(Connection)).Returns(new[] {StrangeAlarm});
                }
                
                public class AndRunInvoked : AndRepositoryActiveContainsSomeStrangeAlarm
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        AlarmsRunTarget.Run();
                    }

                    public class ThenStrangeAlarmIsReseted : AndRunInvoked
                    {
                        [Test]
                        public void Test()
                        {
                            Repository.Verify(r => r.Update(
                                Connection,
                                It.Is<AlarmSampleId[]>(tuples => tuples.Any(t => t.Equals(StrangeAlarm))),
                                TimeService.Object.UtcTime), Times.Once());
                        }
                    }
                }
            }

            public class AndTimeToStoreIsNull : AndProjectContainsTags
            {
                protected override void EstablishContext()
                {
                    TimeToStore = null;
                    base.EstablishContext();
                }

                public class AndRunInvoked : AndTimeToStoreIsNull
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        AlarmsRunTarget.Run();
                    }

                    public class ThenDeleteOldAlarmNotPerfomed : AndRunInvoked
                    {
                        [Test]
                        public void Test()
                        {
                            Repository.Verify(r => r.DeleteNotActive(Connection, It.IsAny<DateTime>()), Times.Never());
                        }
                    }
                }
            }

            public class AndTimeToStoreIsNotNull : AndProjectContainsTags
            {
                protected TimeSpan TimeSpanToStore;

                protected override void EstablishContext()
                {
                    TimeSpanToStore = TimeSpan.FromDays(1);
                    TimeToStore = TimeSpanToStore.Ticks;
                    base.EstablishContext();
                }

                public class AndRunInvoked : AndTimeToStoreIsNotNull
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        AlarmsRunTarget.Run();
                    }

                    public class ThenDeleteOldAlarmPerfomed : AndRunInvoked
                    {
                        [Test]
                        public void Test()
                        {
                            Repository.Verify(r => r.DeleteNotActive(Connection, TimeService.Object.UtcTime - TimeSpanToStore), Times.Once());
                        }
                    }
                }
            }
        }

        public class AndInvokingGetCurrentAlarms : WhenUsingAlarmsRunTarget
        {
            protected CriteriaType CriteriaType;
            protected AlarmSampleId Criteria;
            protected int MaxCount;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                CriteriaType = CriteriaType.DownFromInfinity;
                Criteria = new AlarmSampleId(DateTime.UtcNow, RandomGenerator.GetRandomInt32());
                MaxCount = RandomGenerator.GetRandomInt32();
                AlarmsRunTarget.GetCurrentAlarms(CriteriaType, Criteria, MaxCount);
            }

            public class ThenRepositoryInvoked : AndInvokingGetCurrentAlarms
            {
                [Test]
                public void Test()
                {
                    Repository.Verify(r => r.GetCurrentAlarms(Connection, CriteriaType, Criteria, MaxCount), Times.Once());
                }
            }
        }

        public class AndInvokingGetHistoryAlarms : WhenUsingAlarmsRunTarget
        {
            protected CriteriaType CriteriaType;
            protected AlarmSampleId Criteria;
            protected int MaxCount;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                CriteriaType = CriteriaType.DownFromInfinity;
                Criteria = new AlarmSampleId(DateTime.UtcNow, RandomGenerator.GetRandomInt32());
                MaxCount = RandomGenerator.GetRandomInt32();
                AlarmsRunTarget.GetHistoryAlarms(CriteriaType, Criteria, MaxCount);
            }

            public class ThenRepositoryInvoked : AndInvokingGetHistoryAlarms
            {
                [Test]
                public void Test()
                {
                    Repository.Verify(r => r.GetHistoryAlarms(Connection, CriteriaType, Criteria, MaxCount), Times.Once());
                }
            }
        }

        public class AndInvokingGetStatus : WhenUsingAlarmsRunTarget
        {
            protected bool HasActive;
            protected bool HasUnacknowledged;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Repository.Setup(r => r.HasActiveAlarms(Connection)).Returns(true);
                Repository.Setup(r => r.HasUnacknowledgedAlarms(Connection)).Returns(false);
                var status = AlarmsRunTarget.GetHasActiveAndUnacknowledged();
                HasActive = status.Item1;
                HasUnacknowledged = status.Item2;
            }

            public class ThenRepositoryHasActiveAlarmsInvoked : AndInvokingGetStatus
            {
                [Test]
                public void Test()
                {
                    Repository.Verify(r => r.HasActiveAlarms(Connection), Times.Once());
                    Assert.That(HasActive, Is.True);
                }
            }

            public class ThenRepositoryHasUnacknowledgeAlarmsInvoked : AndInvokingGetStatus
            {
                [Test]
                public void Test()
                {
                    Repository.Verify(r => r.HasUnacknowledgedAlarms(Connection), Times.Once());
                    Assert.That(HasUnacknowledged, Is.False);
                }
            }
        }
    }
}
