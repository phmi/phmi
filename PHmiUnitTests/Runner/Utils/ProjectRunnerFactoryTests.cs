using Moq;
using NUnit.Framework;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClientUnitTests;
using PHmiModel.Interfaces;
using PHmiRunner.Utils;
using PHmiTools.Utils.Npg;

namespace PHmiUnitTests.Runner.Utils
{
    public class WhenUsingProjectRunnerFactory : Specification
    {
        protected IProjectRunnerFactory RunnerFactory;
        protected Mock<IReporter> Reporter;
        protected Mock<ITimeService> TimeService;
        protected Mock<IModelContextFactory> ModelContextFactory = new Mock<IModelContextFactory>();
        protected Mock<IModelContext> ModelContext = new Mock<IModelContext>();
        protected const string ProjectName = "Project";
        protected const string ConnectionString = "ConnectionString";

        protected override void EstablishContext()
        {
            base.EstablishContext();
            TimeService = new Mock<ITimeService>();
            Reporter = new Mock<IReporter>();
            RunnerFactory = new ProjectRunnerFactory(
                TimeService.Object, Reporter.Object, ModelContextFactory.Object, new NpgHelper());
            ModelContextFactory.Setup(f => f.Create(ConnectionString, false)).Returns(ModelContext.Object);
        }

        public class AndExecuteCreate : WhenUsingProjectRunnerFactory
        {
            protected IProjectRunner Runner;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                Runner = RunnerFactory.Create(ProjectName, ConnectionString);
            }

            public class ThenRunnerCreated : AndExecuteCreate
            {
                [Test]
                public void Test()
                {
                    Assert.That(Runner, Is.Not.Null);
                }
            }

            public class ModelContextCreated : AndExecuteCreate
            {
                [Test]
                public void Test()
                {
                    ModelContextFactory.Verify(f => f.Create(ConnectionString, false), Times.Once());
                }
            }
        }
    }
}
