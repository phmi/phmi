using System;
using Moq;
using NUnit.Framework;
using PHmiClient.Alarms;
using PHmiClient.Loc;
using PHmiClient.Utils.Notifications;
using PHmiClient.Wcf;
using PHmiClient.Wcf.ServiceTypes;

namespace PHmiClientUnitTests.Client.Alarms
{
    public class WhenUsingAlarmService : Specification
    {
        internal Mock<IReporter> Reporter;
        internal IAlarmService AlarmService;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            Reporter = new Mock<IReporter>();
            AlarmService = new AlarmService(Reporter.Object);
        }

        public class AndAddedCategories : WhenUsingAlarmService
        {
            protected Mock<AlarmCategoryAbstract> CommonCategory;
            protected Mock<AlarmCategoryAbstract> Category;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                CommonCategory = new Mock<AlarmCategoryAbstract>();
                CommonCategory.Setup(c => c.Id).Returns(0);
                AlarmService.Add(CommonCategory.Object);
                Category = new Mock<AlarmCategoryAbstract>();
                Category.SetupProperty(c => c.HasActive);
                Category.SetupProperty(c => c.HasUnacknowledged);
                Category.Setup(c => c.Id).Returns(1);
                AlarmService.Add(Category.Object);
            }

            public class AndRunInvoked : AndAddedCategories
            {
                internal Mock<IService> Service;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Service = new Mock<IService>();
                    AlarmService.Run(Service.Object);
                }

                public class ThenServiceRemapTagsNotInvoked : AndRunInvoked
                {
                    [Test]
                    public void Test()
                    {
                        Service.Verify(s => s.RemapAlarms(It.IsAny<RemapAlarmsParameter[]>()), Times.Never());
                    }
                }
            }

            public class AndCategoryIsRead : AndAddedCategories
            {
                internal RemapTagsParameter Parameter;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Parameter = new RemapTagsParameter();
                    CommonCategory.Setup(d => d.IsRead()).Returns(true).Verifiable();
                }

                public class AndWeHaveTheService : AndCategoryIsRead
                {
                    internal Mock<IService> Service;
                    internal RemapAlarmResult Result;
                    internal WcfNotification Notification;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        Service = new Mock<IService>();
                        Notification = new WcfNotification
                        {
                            StartTime = DateTime.UtcNow,
                            Message = "Message",
                            ShortDescription = "ShortDescription",
                            LongDescription = "LongDescription"
                        };
                        Result = new RemapAlarmResult
                        {
                            HasActive = true,
                            HasUnacknowledged = false,
                            Current = new Alarm[0][],
                            History = new Alarm[0][],
                            Notifications = new[] { Notification }
                        };
                        Service.Setup(s => s.RemapAlarms(It.Is<RemapAlarmsParameter[]>(p =>
                            p.Length == 1 && p[0].CategoryId == Category.Object.Id && p[0].GetStatus)))
                            .Returns(new[] { Result }).Verifiable();
                    }

                    public class AndRunInvoked1 : AndWeHaveTheService
                    {
                        protected override void EstablishContext()
                        {
                            base.EstablishContext();
                            AlarmService.Run(Service.Object);
                        }

                        public class ThenAlarmServiceCalledWithParameter : AndRunInvoked1
                        {
                            [Test]
                            public void Test()
                            {
                                Service.Verify();
                            }
                        }

                        public class ThenServiceRemapTagsInvoked : AndRunInvoked1
                        {
                            [Test]
                            public void Test()
                            {
                                Service.Verify();
                            }
                        }

                        public class ThenAlarmCategoriesApplyResultInvoked : AndRunInvoked1
                        {
                            [Test]
                            public void Test()
                            {
                                Category.VerifySet(t => t.HasActive = true, Times.Once());
                                CommonCategory.VerifySet(t => t.HasActive = true, Times.Once());
                                Category.VerifySet(t => t.HasUnacknowledged = false, Times.Once());
                                CommonCategory.VerifySet(t => t.HasUnacknowledged = false, Times.Once());
                            }
                        }

                        public class ThenNotificationIsReported : AndRunInvoked1
                        {
                            [Test]
                            public void Test()
                            {
                                Reporter.Verify(r => r.Report(
                                    Notification.StartTime.ToLocalTime() + " " + Notification.Message,
                                    Notification.ShortDescription,
                                    Notification.LongDescription), Times.Once());
                            }
                        }
                    }
                }
            }
        }

        public class ThenNameReturnsTagService : WhenUsingAlarmService
        {
            [Test]
            public void Test()
            {
                Assert.That(AlarmService.Name, Is.EqualTo(Res.AlarmService));
            }
        }
    }
}
