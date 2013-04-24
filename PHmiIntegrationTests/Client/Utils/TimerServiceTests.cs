using NUnit.Framework;
using PHmiClient.Utils;
using System;
using System.Threading;
using PHmiClientUnitTests;

namespace PHmiIntegrationTests.Client.Utils
{
    public class WhenUsingTimerService : Specification
    {
        protected ITimerService CyclicInvoker;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            CyclicInvoker = new TimerService();
        }

        public class AndSettingTimeSpan : WhenUsingTimerService
        {
            protected TimeSpan Pause;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Pause = TimeSpan.FromMilliseconds(10);
                CyclicInvoker.TimeSpan = Pause;
            }

            public class AndSettingAction : AndSettingTimeSpan
            {
                protected int InvokeCount;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    CyclicInvoker.Elapsed += (obj, arg) => Invoke();
                }

                private void Invoke()
                {
                    InvokeCount++;
                }

                public class AndStartingAndLaterStopping : AndSettingAction
                {
                    protected override void EstablishContext()
                    {
                        base.EstablishContext();
                        CyclicInvoker.Start();
                        Thread.Sleep(Pause.Milliseconds*3);
                        CyclicInvoker.Stop();
                    }

                    public class ThenShouldInvoke : AndStartingAndLaterStopping
                    {
                        [Test]
                        public void Test()
                        {
                            Assert.That(InvokeCount, Is.GreaterThanOrEqualTo(1));
                        }
                    }
                }
            }
        }
    }
}
