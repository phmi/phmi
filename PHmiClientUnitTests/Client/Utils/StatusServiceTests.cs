using System;
using Moq;
using NUnit.Framework;
using PHmiClient.Utils;

namespace PHmiClientUnitTests.Client.Utils
{
    public class WhenUsingStatusService : Specification
    {
        protected IStatusService StatusService;
        protected TimeSpan LifeTime;
        protected Mock<ITimerService> Timer;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            Timer = new Mock<ITimerService>();
            StatusService = new StatusService(Timer.Object);
            LifeTime = TimeSpan.FromSeconds(15);
        }

        public class ThenDefaultLifeTimeShouldBe15Seconds : WhenUsingStatusService
        {
            [Test]
            public void Test()
            {
                Timer.VerifySet(t => t.TimeSpan = LifeTime);
                Timer.Setup(t => t.TimeSpan).Returns(LifeTime);
                Assert.That(StatusService.LifeTime, Is.EqualTo(LifeTime));
            }
        }

        public class AndSettingLifeTime : WhenUsingStatusService
        {
            protected TimeSpan NewLifeTime;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                NewLifeTime = TimeSpan.FromSeconds(30);
                StatusService.LifeTime = NewLifeTime;
            }

            public class ThenTimerTimeSpanShouldEqualToLifeTime : AndSettingLifeTime
            {
                [Test]
                public void Test()
                {
                    Timer.VerifySet(t => t.TimeSpan = NewLifeTime);
                }
            }
        }
        
        public class ThenItShouldImplementINotifyPropertyChanged : WhenUsingStatusService
        {
            [Test]
            public void TestLifeTime()
            {
                NotifyPropertyChangedTester.Test(StatusService, s => s.LifeTime, TimeSpan.FromSeconds(0));
            }

            [Test]
            public void TestMessage()
            {
                NotifyPropertyChangedTester.Test(StatusService, s => s.Message, string.Empty);
            }
        }

        public class ThenMessageShouldBeNull : WhenUsingStatusService
        {
            [Test]
            public void Test()
            {
                Assert.That(StatusService.Message, Is.Null);
            }
        }

        public class AndSettingMessage : WhenUsingStatusService
        {
            protected const string Message = "Message";

            protected override void EstablishContext()
            {
                base.EstablishContext();
                StatusService.Message = Message;
            }

            public class ThenTimerShouldStart : AndSettingMessage
            {
                [Test]
                public void Test()
                {
                    Timer.Verify(t => t.Start(), Times.Once());
                }
            }

            public class AndAfterLifeTimeElapsed : AndSettingMessage
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Timer.Raise(t => t.Elapsed += null, EventArgs.Empty);
                }

                public class ThenMessageShouldBeSetToNull : AndAfterLifeTimeElapsed
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(StatusService.Message, Is.Null);
                    }
                }
            }

            public class AndSettingMessageAgain : AndSettingMessage
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    StatusService.Message = Message;
                }

                public class ThenTimerShouldRestart : AndSettingMessageAgain
                {
                    [Test]
                    public void Test()
                    {
                        Timer.Verify(t => t.Stop());
                        Timer.Verify(t => t.Start());
                    }
                }
            }
        }
    }
}
