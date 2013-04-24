using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using PHmiClient.Utils;
using PHmiClientUnitTests;

namespace PHmiIntegrationTests.Client.Utils
{
    public class WhenUsingTimeService : Specification
    {
        protected ITimeService TimeService;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            TimeService = new TimeService();
        }

        public class ThenUtcTimeReturnsCurrentTime : WhenUsingTimeService
        {
            [Test]
            public void Test()
            {
                Assert.That(Math.Abs((TimeService.UtcTime - DateTime.UtcNow).TotalSeconds), Is.LessThan(10));
            }
        }

        public class AndSettingTime : WhenUsingTimeService
        {
            protected DateTime SettedTime;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                SettedTime = DateTime.UtcNow + TimeSpan.FromDays(10.9999999999);
                TimeService.UtcTime = SettedTime;
            }

            public class ThenUtcTimeReturnsSettedTime : AndSettingTime
            {
                [Test]
                public void Test()
                {
                    Assert.That(Math.Abs((TimeService.UtcTime - SettedTime).TotalSeconds), Is.LessThan(10));
                }
            }
        }
    }
}
