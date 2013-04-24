using System.ComponentModel;
using Moq;
using NUnit.Framework;
using PHmiClient.Utils;
using PHmiClient.Utils.ViewInterfaces;
using PHmiTools;
using PHmiTools.Dialogs.Project;
using PHmiTools.Utils;
using PHmiTools.Utils.Npg;

namespace PHmiUnitTests.Tools.Dialogs.Project
{
    [TestFixture]
    public class ProjectDialogViewModelTests
    {
        #region Stub

        public class Service : IProjectDialogService
        {
            public readonly Mock<IDialogHelper> DialogHelperStub = new Mock<IDialogHelper>();

            public readonly Mock<IConnectionStringHelper> ConnectionStringHelperStub = new Mock<IConnectionStringHelper>();

            public readonly Mock<INpgConnectionParameters> ConnectionParametersStub = new Mock<INpgConnectionParameters>();

            public IDialogHelper DialogHelper
            {
                get { return DialogHelperStub.Object; }
            }

            public IConnectionStringHelper ConnectionStringHelper
            {
                get { return ConnectionStringHelperStub.Object; }
            }

            public INpgConnectionParameters ConnectionParameters
            {
                get { return ConnectionParametersStub.Object; }
            }
        }

        private class ProjectDialogViewModel : PHmiTools.Dialogs.Project.ProjectDialogViewModel
        {
            public ProjectDialogViewModel(IProjectDialogService service) : base(service) { }

            public void SetInProgress(bool value)
            {
                InProgress = value;
            }

            public void SetProgress(int value)
            {
                Progress = value;
            }

            public void SetProgressMax(int value)
            {
                ProgressMax = value;
            }

            public void SetProgressIsIndeterminate(bool value)
            {
                ProgressIsIndeterminate = value;
            }
        }

        private Service _service;
        private ProjectDialogViewModel _viewModel;
        private Mock<IWindow> _windowStub;
        
        #endregion

        private void CreateViewModel()
        {
            _viewModel = new ProjectDialogViewModel(_service) {View = _windowStub.Object};
        }

        [SetUp]
        public void SetUp()
        {
            _service = new Service();
            _windowStub = new Mock<IWindow>();
            CreateViewModel();
        }

        #region ConnectionParameters

        [Test]
        public void ConnectionParametersReturnsConfigParameters()
        {
            _service.ConnectionStringHelperStub
                .Setup(helper => helper.Get(PHmiConstants.PHmiConnectionStringName))
                .Returns("Connection String");
            CreateViewModel();
            _service.ConnectionParametersStub.Verify(p => p.Update("Connection String"));
        }

        [Test]
        public void ConnectionParametersReturnsDefaultConfigParametersIfConfigEmpty()
        {
            _service.ConnectionStringHelperStub
                .Setup(helper => helper.Get(PHmiConstants.PHmiConnectionStringName))
                .Returns((string)null);
            _service.ConnectionParametersStub.VerifySet(p => p.Server = "localhost", Times.Once());
            _service.ConnectionParametersStub.VerifySet(p => p.Port = "5432", Times.Once());
            _service.ConnectionParametersStub.VerifySet(p => p.UserId = "postgres", Times.Once());
        }

        #endregion

        #region Remember

        private static readonly object[] BooleanCases = new object[] {true, false};

        [Test, TestCaseSource("BooleanCases")]
        public void RememberSetRaisesPropertyChanged(bool value)
        {
            var raised = false;
            _viewModel.PropertyChanged += (sender, args) => { raised = true; };
            _viewModel.Remember = value;
            Assert.IsTrue(raised);
        }

        [Test]
        public void RememberIsFalseWhenConfigDoesNotContainConnectionString()
        {
            _service.ConnectionStringHelperStub
                   .Setup(helper => helper.Get(PHmiConstants.PHmiConnectionStringName))
                   .Returns((string)null);
            CreateViewModel();
            Assert.IsFalse(_viewModel.Remember);
        }

        [Test]
        public void RememberIsTrueWhenConfigContainsConnectionString()
        {
            _service.ConnectionStringHelperStub
                   .Setup(helper => helper.Get(PHmiConstants.PHmiConnectionStringName))
                   .Returns("NotNull");
            CreateViewModel();
            Assert.IsTrue(_viewModel.Remember);
        }

        #endregion

        #region OkCommandCanExecute

        [Test, TestCaseSource("_okCommandCanExecuteTestCases")]
        public void OkCommandCanExecuteTest(bool paramsHasError, bool inProgress, bool canExecute)
        {
            _service.ConnectionParametersStub.Setup(p => p.Error).Returns(paramsHasError ? "Error!" : string.Empty);
            _viewModel.SetInProgress(inProgress);
            Assert.AreEqual(canExecute, _viewModel.OkCommand.CanExecute(null));
        }

        private readonly object[] _okCommandCanExecuteTestCases =
            {
                new object[] {false, false, true},
                new object[] {true, false, false},
                new object[] {false, true, false},
                new object[] {true, true, false}
            };

        [Test]
        public void OkCommandCanExecuteChangedRaisesWhenConnectionParameterPropertyChangedRaised()
        {
            var raised = false;
            _viewModel.OkCommand.CanExecuteChanged += (sender, args) => { raised = true; };
            _service.ConnectionParametersStub.Raise(p => p.PropertyChanged += null, new PropertyChangedEventArgs(null));
            Assert.IsTrue(raised);
        }

        #endregion

        #region OkCommand

        [Test]
        public void OkCommandSetsDialogResultTrue()
        {
            _viewModel.OkCommand.Execute(null);
            _windowStub.VerifySet(w => w.DialogResult = true, Times.Once());
        }

        [Test]
        public void OkCommandDoesNotSetDialogResultIfItWasCanceled()
        {
            _viewModel.CancelCommand.Execute(null);
            _viewModel.OkCommand.Execute(null);
            _windowStub.VerifySet(w => w.DialogResult = true, Times.Never());
        }

        [Test]
        public void OkCommandSavesConnectionStringIfRememberIsTrue()
        {
            _viewModel.Remember = true;
            _viewModel.OkCommand.Execute(null);
            _service.ConnectionStringHelperStub
                .Verify(helper => helper.Set(
                    PHmiConstants.PHmiConnectionStringName, _viewModel.ConnectionParameters.ConnectionString),
                Times.Once());
        }

        [Test]
        public void OkCommandRemovesConnectionStringIfRememberIsFalse()
        {
            _viewModel.Remember = false;
            _viewModel.OkCommand.Execute(null);
            _service.ConnectionStringHelperStub.Verify(helper => helper.Set(PHmiConstants.PHmiConnectionStringName, null), Times.Once());
        }

        #endregion

        #region CancelCommand

        [Test]
        public void CancelCommandSetsDialogResultFalse()
        {
            _viewModel.CancelCommand.Execute(null);
            _windowStub.VerifySet(w => w.DialogResult = false, Times.Once());
        }

        [Test]
        public void CancelCommandDoesNotSetDialogResultFalseIfItWasOked()
        {
            _viewModel.OkCommand.Execute(null);
            _viewModel.CancelCommand.Execute(null);
            _windowStub.VerifySet(w => w.DialogResult = false, Times.Never());
        }

        [Test]
        public void CancelCommandDoesNotSetConnectionStringIfRememberIsTrue()
        {
            _viewModel.Remember = true;
            _viewModel.CancelCommand.Execute(null);
            _service.ConnectionStringHelperStub
                .Verify(helper => helper.Set(PHmiConstants.PHmiConnectionStringName, It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void CancelCommandRemovesConnectionStringIfRememberIsFalse()
        {
            _viewModel.Remember = false;
            _viewModel.CancelCommand.Execute(null);
            _service.ConnectionStringHelperStub.Verify(helper => helper.Set(PHmiConstants.PHmiConnectionStringName, null), Times.Once());
        }

        #endregion

        #region Progress

        [Test, TestCaseSource("_booleanCases")]
        public void InProgressSetRaisesPropertyChanged(bool value)
        {
            var raised = false;
            _viewModel.PropertyChanged += (sender, args) =>
                {
                    raised = true;
                    Assert.AreEqual("InProgress", args.PropertyName);
                };
            _viewModel.SetInProgress(value);
            Assert.IsTrue(raised);
        }

        [Test, TestCaseSource("_intValues")]
        public void ProgressSetRaisesPropertyChanged(int value)
        {
            var raised = false;
            _viewModel.PropertyChanged += (sender, args) =>
                {
                    raised = true;
                    Assert.AreEqual("Progress", args.PropertyName);
                };
            _viewModel.SetProgress(value);
            Assert.IsTrue(raised);
        }

        [Test, TestCaseSource("_intValues")]
        public void ProgressMaxSetRaisesPropertyChanged(int value)
        {
            var raised = false;
            _viewModel.PropertyChanged += (sender, args) =>
                {
                    raised = true;
                    Assert.AreEqual("ProgressMax", args.PropertyName);
                };
            _viewModel.SetProgressMax(value);
            Assert.IsTrue(raised);
        }

        [Test, TestCaseSource("_booleanCases")]
        public void ProgressIsIndeterminateSetRaisesPropertyChanged(bool value)
        {
            var raised = false;
            _viewModel.PropertyChanged += (sender, args) =>
                {
                    raised = true;
                    Assert.AreEqual("ProgressIsIndeterminate", args.PropertyName);
                };
            _viewModel.SetProgressIsIndeterminate(value);
            Assert.IsTrue(raised);
        }

        private readonly int[] _intValues = {int.MinValue, int.MaxValue, 0};

        [Test, TestCaseSource("_booleanCases")]
        public void InProgressSetRaisesOkCommandCanExecuteChanged(bool value)
        {
            var raised = false;
            _viewModel.OkCommand.CanExecuteChanged += (sender, args) => { raised = true; };
            _viewModel.SetInProgress(value);
            Assert.IsTrue(raised);
        }

        private readonly bool[] _booleanCases = {true, false};

        #endregion
    }
}
