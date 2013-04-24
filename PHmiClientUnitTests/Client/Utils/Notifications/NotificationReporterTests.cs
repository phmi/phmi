using Moq;
using NUnit.Framework;
using PHmiClient.Utils;
using System;
using System.Linq;
using PHmiClient.Utils.Notifications;

namespace PHmiClientUnitTests.Client.Utils.Notifications
{
    public class WhenUsingNotificationReporter : Specification
    {
        protected INotificationReporter NotificationReporter;
        protected Mock<ITimeService> TimeServiceMock;
        protected Mock<ITimerService> TimerServiceMock;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            TimeServiceMock = new Mock<ITimeService>();
            TimerServiceMock = new Mock<ITimerService>();
            NotificationReporter = new NotificationReporter(TimeServiceMock.Object, TimerServiceMock.Object);
        }

        public class ThenTimerShouldStart : WhenUsingNotificationReporter
        {
            [Test]
            public void Test()
            {
                TimerServiceMock.Verify(t => t.Start(), Times.Once());
            }
        }

        public class AndInvokedUnaryReport : WhenUsingNotificationReporter
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                CatchExceptionInEstablishContext = true;
                NotificationReporter.Report("Message");
            }

            public class ThenNoExceptionIsThrown : AndInvokedUnaryReport
            {
                [Test]
                public void Test()
                {
                    Assert.That(ThrownException, Is.Null);
                }
            }
        }

        public class AndInvokedReport : WhenUsingNotificationReporter
        {
            protected readonly DateTime StartTime = DateTime.UtcNow;
            protected const string Message = "message";
            protected const string ShortDescription = "shortDescription";
            protected const string LongDescription = "longDescription";
            protected Notification Notification;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                TimeServiceMock.Setup(s => s.UtcTime).Returns(StartTime);
                NotificationReporter.Report(Message, ShortDescription, LongDescription);
                Notification = NotificationReporter.Notifications.SingleOrDefault();
            }

            public class ThenNotificationsShouldContainNotification : AndInvokedReport
            {
                [Test]
                public void Test()
                {
                    Assert.That(Notification, Is.Not.Null);
                    Assert.That(Notification.StartTime, Is.EqualTo(StartTime));
                    Assert.That(Notification.Message, Is.EqualTo(Message));
                    Assert.That(Notification.ShortDescription, Is.EqualTo(ShortDescription));
                    Assert.That(Notification.LongDescription, Is.EqualTo(LongDescription));
                }
            }

            public class ThenContainsActiveNotificationsIsTrue : AndInvokedReport
            {
                [Test]
                public void Test()
                {
                    Assert.That(NotificationReporter.ContainsActiveNotifications, Is.True);
                }
            }

            public class AndResetNotificationInvoked : AndInvokedReport
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    NotificationReporter.Reset(Notification);
                }

                public class ThenNotificationStays : AndResetNotificationInvoked
                {
                    [Test]
                    public void Test()
                    {
                        Assert.Contains(Notification, NotificationReporter.Notifications);
                    }
                }
            }

            public class AndResetAllInvoked : AndInvokedReport
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    NotificationReporter.ResetAll();
                }

                public class ThenNotificationStays : AndResetNotificationInvoked
                {
                    [Test]
                    public void Test()
                    {
                        Assert.Contains(Notification, NotificationReporter.Notifications);
                    }
                }
            }

            public class AndExpirationTimeElapsed : AndInvokedReport
            {
                protected DateTime ExpirationTime;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    ExpirationTime = StartTime.Add(NotificationReporter.ExpirationTime).AddTicks(1);
                    TimeServiceMock.Setup(s => s.UtcTime).Returns(ExpirationTime);
                    TimerServiceMock.Raise(s => s.Elapsed += null, EventArgs.Empty);
                }

                public class ThenNotificationShouldHaveEndTime : AndExpirationTimeElapsed
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(Notification.EndTime, Is.EqualTo(StartTime));
                    }
                }

                public class ThenContainsActiveNotificationsIsFalseAgain : AndExpirationTimeElapsed
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(NotificationReporter.ContainsActiveNotifications, Is.False);
                    }
                }

                public class AndLifeTimeElapsed : AndExpirationTimeElapsed
                {
                    protected DateTime LifeTime;

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        LifeTime = ExpirationTime.Add(NotificationReporter.LifeTime).AddTicks(1);
                        TimeServiceMock.Setup(s => s.UtcTime).Returns(LifeTime);
                        TimerServiceMock.Raise(s => s.Elapsed += null, EventArgs.Empty);
                    }

                    public class ThenNotificationShouldDissapear : AndLifeTimeElapsed
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(NotificationReporter.Notifications, Is.Empty);
                        }
                    }
                }

                public class AndNowResetNotificationInvoked : AndExpirationTimeElapsed
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        NotificationReporter.Reset(Notification);
                    }

                    public class ThenNotificationIsRemoved : AndNowResetNotificationInvoked
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(NotificationReporter.Notifications, Is.Empty);
                        }
                    }
                }

                public class AndNowResetAllInvoked : AndExpirationTimeElapsed
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        NotificationReporter.ResetAll();
                    }

                    public class ThenNotificationIsRemoved : AndNowResetAllInvoked
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(NotificationReporter.Notifications, Is.Empty);
                        }
                    }
                }

                public class AndNowResetNonExistentNotificationInvoked : AndExpirationTimeElapsed
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        NotificationReporter.Reset(new Notification(StartTime, Message, ShortDescription, LongDescription)
                            {
                                EndTime = ExpirationTime
                            });
                    }

                    public class ThenNotificationRemains : AndNowResetNonExistentNotificationInvoked
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.Contains(Notification, NotificationReporter.Notifications);
                        }
                    }
                }
            }

            public class AndReportedAgainTheSameNotification : AndInvokedReport
            {
                protected DateTime LaterTime;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    LaterTime = StartTime.Add(TimeSpan.FromSeconds(1));
                    TimeServiceMock.Setup(s => s.UtcTime).Returns(LaterTime);
                    NotificationReporter.Report("message", "shortDescription", "longDescription");
                }

                public class ThenNotificationsDoesNotChange : AndReportedAgainTheSameNotification
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(NotificationReporter.Notifications.Count, Is.EqualTo(1));
                    }
                }

                public class AndExpirationTimeElapsedFromFirstReport : AndReportedAgainTheSameNotification
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        var now = StartTime.Add(NotificationReporter.ExpirationTime).AddTicks(1);
                        TimeServiceMock.Setup(s => s.UtcTime).Returns(now);
                        TimerServiceMock.Raise(s => s.Elapsed += null, EventArgs.Empty);
                    }

                    public class ThenNotificationShouldNotHaveEndTime : AndExpirationTimeElapsedFromFirstReport
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Notification.EndTime, Is.Null);
                        }
                    }

                    public class ThenContainsActiveNotificationsIsStillTrue : AndExpirationTimeElapsedFromFirstReport
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(NotificationReporter.ContainsActiveNotifications, Is.True);
                        }
                    }
                }

                public class AndExpirationTimeElapsedFromSecondReport : AndReportedAgainTheSameNotification
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        var now = LaterTime.Add(NotificationReporter.ExpirationTime).AddTicks(1);
                        TimeServiceMock.Setup(s => s.UtcTime).Returns(now);
                        TimerServiceMock.Raise(s => s.Elapsed += null, EventArgs.Empty);
                    }

                    public class ThenNotificationShouldHaveEndTime : AndExpirationTimeElapsedFromSecondReport
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(Notification.EndTime, Is.EqualTo(LaterTime));
                        }
                    }

                    public class ThenContainsActiveNotificationsIsFalseNow : AndExpirationTimeElapsedFromSecondReport
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(NotificationReporter.ContainsActiveNotifications, Is.False);
                        }
                    }
                }
            }
        }

        public class AndInvokedReportExeption : WhenUsingNotificationReporter
        {
            protected readonly DateTime StartTime = DateTime.UtcNow;
            protected Exception Exception;
            protected Notification Notification;
            protected const string Message = "message";

            protected override void EstablishContext()
            {
                base.EstablishContext();
                TimeServiceMock.Setup(s => s.UtcTime).Returns(StartTime);
                Exception = new Exception("exception message");
                NotificationReporter.Report(Message, Exception);
                Notification = NotificationReporter.Notifications.SingleOrDefault();
            }

            public class ThenNotificationsShouldContainNotification : AndInvokedReportExeption
            {
                [Test]
                public void Test()
                {
                    Assert.That(Notification, Is.Not.Null);
                    Assert.That(Notification.StartTime, Is.EqualTo(StartTime));
                    Assert.That(Notification.Message, Is.EqualTo(Message));
                    Assert.That(Notification.ShortDescription, Is.EqualTo(Exception.Message));
                    Assert.That(Notification.LongDescription, Is.EqualTo(Exception.ToString()));
                }
            }
        }

        public class ThenContainsActiveNotificationsIsFalse : WhenUsingNotificationReporter
        {
            [Test]
            public void Test()
            {
                Assert.That(NotificationReporter.ContainsActiveNotifications, Is.False);
            }
        }
    }
}
