using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using PHmiClient.Utils.Notifications;
using PHmiClientUnitTests;
using PHmiResources.Loc;
using PHmiRunner.Utils;
using PHmiTools.Utils.Npg;

namespace PHmiUnitTests.Runner.Utils
{
    public class WhenUsingDataDbCreator : Specification
    {
        protected Mock<INpgHelper> NpgHelper;
        protected string ConnectionString;
        protected Mock<IReporter> Reporter;
        protected DataDbCreator Creator;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            NpgHelper = new Mock<INpgHelper>();
            ConnectionString = "ConnectionString";
            Reporter = new Mock<IReporter>();
            Creator = new DataDbCreator(ConnectionString, NpgHelper.Object, Reporter.Object);
            NpgHelper.Setup(h => h.GetDatabase(ConnectionString)).Returns("DB");
        }

        public class AndDbDoesNotExist : WhenUsingDataDbCreator
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                NpgHelper.Setup(h => h.DatabaseExists(ConnectionString)).Returns(false);
            }

            public class AndStarted : AndDbDoesNotExist
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Creator.Start();
                }

                public class ThenDbIsCreated : AndStarted
                {
                    [Test]
                    public void Test()
                    {
                        NpgHelper.Verify(h => h.CreateDatabase(ConnectionString), Times.Once());
                    }

                    [Test]
                    public void ReporterReported()
                    {
                        Reporter.Verify(r => r.Report(string.Format(Res.DatabaseCreatedMessage, "DB"), null, null), Times.Once());
                    }
                }
            }
        }

        public class AndDbExists : WhenUsingDataDbCreator
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                NpgHelper.Setup(h => h.DatabaseExists(ConnectionString)).Returns(true);
            }

            public class AndStarted : AndDbExists
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    Creator.Start();
                }

                public class ThenDbIsNotCreated : AndStarted
                {
                    [Test]
                    public void Test()
                    {
                        NpgHelper.Verify(h => h.CreateDatabase(ConnectionString), Times.Never());
                    }
                }
            }
        }
    }
}
