using System;
using Moq;
using NUnit.Framework;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.ViewInterfaces;
using PHmiClientUnitTests;
using PHmiClientUnitTests.Stubs;
using PHmiRunner;
using PHmiRunner.Utils;
using PHmiTools;
using PHmiTools.Utils.Npg;

namespace PHmiUnitTests.Runner
{
    public class WhenUsingMainWindowViewModel : Specification
    {
        protected MainWindowViewModel ViewModel;
        protected IActionHelper ActionHelper;
        protected Mock<ITimeService> TimeService;
        protected Mock<IProjectRunnerFactory> RunnerFactory;
        protected Mock<IProjectRunner> Runner;
        protected Mock<INotificationReporter> NotificationReporter;
        protected const string Database = "Database";
        protected const string ConnectionString = "ConnectionString";
        protected Mock<IWindow> Window;

        protected override void EstablishContext()
        {
            base.EstablishContext();
            ActionHelper = new ActionHelperStub();
            TimeService = new Mock<ITimeService>();
            NotificationReporter = new Mock<INotificationReporter>();
            RunnerFactory = new Mock<IProjectRunnerFactory>();
            Runner = new Mock<IProjectRunner>();
            RunnerFactory.Setup(f => f.Create(Database, ConnectionString)).Returns(Runner.Object);
            Window = new Mock<IWindow>();
            ViewModel = new MainWindowViewModel(
                ActionHelper, TimeService.Object, NotificationReporter.Object, RunnerFactory.Object)
                {
                    View = Window.Object
                };
        }

        public class ThenNotificationReporterExpirationTimeSetToOneSecond : WhenUsingMainWindowViewModel
        {
            [Test]
            public void Test()
            {
                NotificationReporter.VerifySet(r => r.ExpirationTime = TimeSpan.FromSeconds(1));
            }
        }

        public class AndSettingConnectionParametersThenChangedRaised : WhenUsingMainWindowViewModel
        {
            protected int Counter;
            protected Mock<INpgConnectionParameters> ConnectionParameters;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                ConnectionParameters = new Mock<INpgConnectionParameters>();
                ConnectionParameters.Setup(p => p.Database).Returns(Database);
                ConnectionParameters.Setup(p => p.ConnectionString).Returns(ConnectionString);
                Counter = 0;
            }

            private void RegisterPropertyChangedEventFor(string property)
            {
                ViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == property)
                    {
                        Counter++;
                    }
                };
            }

            private void SetConnectionParameters()
            {
                ViewModel.ConnectionParameters = ConnectionParameters.Object;
            }

            private void AssertPropertyChangedOnce()
            {
                Assert.That(Counter, Is.EqualTo(1));
            }

            private void TestProperty(string property)
            {
                RegisterPropertyChangedEventFor(property);
                SetConnectionParameters();
                AssertPropertyChangedOnce();
            }

            [Test]
            public void ForConnectionParameters()
            {
                TestProperty("ConnectionParameters");
            }

            [Test]
            public void ForTitle()
            {
                TestProperty("Title");
            }

            [Test]
            public void TestCloseCanExecuteChanged()
            {
                ViewModel.CloseCommand.CanExecuteChanged += (sender, args) =>
                {
                    Counter++;
                };

                SetConnectionParameters();
                AssertPropertyChangedOnce();
            }
        }

        public class ThenTitleReturnPHmiRunner : WhenUsingMainWindowViewModel
        {
            [Test]
            public void Test()
            {
                Assert.That(ViewModel.Title, Is.EqualTo(PHmiConstants.PHmiRunnerName));
            }
        }

        public class AndSettingConnectionParameters : WhenUsingMainWindowViewModel
        {
            protected Mock<INpgConnectionParameters> ConnectionParameters;

            protected override void EstablishContext()
            {
                base.EstablishContext();
                ConnectionParameters = new Mock<INpgConnectionParameters>();
                ConnectionParameters.SetupGet(c => c.Database).Returns(Database);
                ConnectionParameters.SetupGet(c => c.ConnectionString).Returns(ConnectionString);
                ViewModel.ConnectionParameters = ConnectionParameters.Object;
            }

            public class ThenTitleReturnsDatabaseName : AndSettingConnectionParameters
            {
                [Test]
                public void Test()
                {
                    Assert.That(ViewModel.Title, Is.EqualTo(string.Format("{0} - {1}", Database, PHmiConstants.PHmiRunnerName)));
                }
            }

            public class ThenRunnerIsCreatedAndStarted : AndSettingConnectionParameters
            {
                [Test]
                public void Test()
                {
                    RunnerFactory.Verify(f => f.Create(Database, ConnectionString), Times.Once());
                    Runner.Verify(r => r.Start(), Times.Once());
                }
            }

            public class AndSettingConenctionParametersToNull : AndSettingConnectionParameters
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    ViewModel.ConnectionParameters = null;
                }

                public class ThenRunnerIsStopped : AndSettingConenctionParametersToNull
                {
                    [Test]
                    public void Test()
                    {
                        Runner.Verify(r => r.Stop(), Times.Once());
                    }
                }

                public class ThenTitleReturnsPHmiRunner : AndSettingConenctionParametersToNull
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(ViewModel.Title, Is.EqualTo(PHmiConstants.PHmiRunnerName));
                    }
                }
            }

            public class AndSettingConnectionParametersToAnother : AndSettingConnectionParameters
            {
                protected Mock<INpgConnectionParameters> AnotherConnectionParameters;
                protected const string AnotherDatabase = "AnotherDatabase";
                protected const string AnotherConnectionString = "AnotherConnectionString";
                protected Mock<IProjectRunner> AnotherRunner;

                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    AnotherConnectionParameters = new Mock<INpgConnectionParameters>();
                    AnotherRunner = new Mock<IProjectRunner>();
                    AnotherConnectionParameters.SetupGet(c => c.Database).Returns(AnotherDatabase);
                    AnotherConnectionParameters.SetupGet(c => c.ConnectionString).Returns(AnotherConnectionString);
                    RunnerFactory.Setup(f => f.Create(AnotherDatabase, AnotherConnectionString)).Returns(AnotherRunner.Object);
                    ViewModel.ConnectionParameters = AnotherConnectionParameters.Object;
                }

                public class ThenTitleReturnsAnotherDatabaseName : AndSettingConnectionParametersToAnother
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(ViewModel.Title, Is.EqualTo(
                            string.Format("{0} - {1}", AnotherDatabase, PHmiConstants.PHmiRunnerName)));
                    }
                }

                public class ThenRunnerIsStopped : AndSettingConnectionParametersToAnother
                {
                    [Test]
                    public void Test()
                    {
                        Runner.Verify(r => r.Stop(), Times.Once());
                    }
                }

                public class ThenAnotherRunnerIsStarted : AndSettingConnectionParametersToAnother
                {
                    [Test]
                    public void Test()
                    {
                        AnotherRunner.Verify(r => r.Start(), Times.Once());
                    }
                }
            }

            public class AndExecutingCloseCommand : AndSettingConnectionParameters
            {
                protected override void EstablishContext()
                {
                    base.EstablishContext();
                    ViewModel.CloseCommand.Execute(null);
                }

                public class ThenConnectionParametersAreSetToNull : AndExecutingCloseCommand
                {
                    [Test]
                    public void Test()
                    {
                        Assert.That(ViewModel.ConnectionParameters, Is.Null);
                    }
                }
            }

            public class ThenCloseCommandCanExecuteIsTrue : AndSettingConnectionParameters
            {
                [Test]
                public void Test()
                {
                    Assert.That(ViewModel.CloseCommand.CanExecute(null), Is.True);
                }
            }
        }

        public class AndExecutingExitCommand : WhenUsingMainWindowViewModel
        {
            protected override void EstablishContext()
            {
                base.EstablishContext();
                ViewModel.ExitCommand.Execute(null);
            }

            public class ThenWindowClosed : AndExecutingExitCommand
            {
                [Test]
                public void Test()
                {
                    Window.Verify(w => w.Close(), Times.Once());
                }
            }
        }

        public class ThenCloseCommandCanExecuteIsFalse : WhenUsingMainWindowViewModel
        {
            [Test]
            public void Test()
            {
                Assert.That(ViewModel.CloseCommand.CanExecute(null), Is.False);
            }
        }
    }
}
