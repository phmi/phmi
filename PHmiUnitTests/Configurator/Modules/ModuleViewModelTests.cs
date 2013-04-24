using System;
using System.ComponentModel;
using System.Windows;
using Moq;
using NUnit.Framework;
using PHmiClient.Utils;
using PHmiClientUnitTests.Stubs;
using PHmiConfigurator.Modules;
using PHmiModel.Interfaces;
using PHmiTools.Utils;

namespace PHmiUnitTests.Configurator.Modules
{
    [TestFixture]
    public class ModuleViewModelTests
    {
        #region Stubs

        public class Service : IModuleService
        {
            public Mock<IDialogHelper> DialogHelperStub = new Mock<IDialogHelper>();

            public Mock<IModelContextFactory> ContextFactoryStub = new Mock<IModelContextFactory>();

            public Mock<IEditorHelper> EditorHelperStub = new Mock<IEditorHelper>();

            public Mock<IClipboardHelper> ClipboardHelperStub = new Mock<IClipboardHelper>();

            private readonly IActionHelper _actionHelper = new ActionHelperStub();

            public IDialogHelper DialogHelper
            {
                get { return DialogHelperStub.Object; }
            }

            public IModelContextFactory ContextFactory
            {
                get { return ContextFactoryStub.Object; }
            }

            public IEditorHelper EditorHelper
            {
                get { return EditorHelperStub.Object; }
            }

            public IClipboardHelper ClipboardHelper
            {
                get { return ClipboardHelperStub.Object; }
            }

            public IActionHelper ActionHelper
            {
                get { return _actionHelper; }
            }
        }
        
        private class ModuleViewModel : PHmiConfigurator.Modules.ModuleViewModel
        {
            public ModuleViewModel(Service service) : base(service) { }

            private bool _isValid;

            public override string Name
            {
                get { return "Name"; }
            }

            public string ErrorMessage;

            public override string Error { get { return ErrorMessage; } }

            public override bool IsValid
            {
                get
                {
                    return base.IsValid && _isValid;
                }
            }

            public bool ThrowOnPostReloadAction;

            protected override void PostReloadAction()
            {
                if (ThrowOnPostReloadAction)
                    throw new Exception("ReloadException");
            }

            public void SetIsValid(bool value)
            {
                _isValid = value;
            }

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

        #endregion

        private Service _service;
        private Mock<IModelContext> _contextStub;
        private ModuleViewModel _viewModel;

        #region TestCases

        private static readonly object[] BooleanTestCases = new object[] { true, false };

        #endregion

        [SetUp]
        public void SetUp()
        {
            _service = new Service();
            _contextStub = new Mock<IModelContext>();
            _service.ContextFactoryStub.Setup(c => c.Create("ConnectionString")).Returns(_contextStub.Object);
            _viewModel = new ModuleViewModel(_service) { ConnectionString = "ConnectionString" };
        }

        #region CloseCommand

        [Test]
        public void CloseCommandRaisesClosedEvent()
        {
            var raised = false;
            _viewModel.Closed += (sender, args) => { raised = true; };
            _viewModel.CloseCommand.Execute(null);
            Assert.IsTrue(raised);
        }

        private static readonly object[] CloseWhenContextHasChangesTestCases =
            new object[]
                {
                    new object[] {false, false},
                    new object[] {true, false},
                    new object[] {null, false},
                    new object[] {false, true},
                    new object[] {true, true},
                    new object[] {null, true}
                };

        [Test, TestCaseSource("CloseWhenContextHasChangesTestCases")]
        public void CloseWhenContextHasChangesTest(bool? messageResult, bool isValid)
        {
            var raised = false;
            _viewModel.Closed += (sender, args) => { raised = true; };
            _viewModel.Reload();
            _contextStub.Setup(c => c.HasChanges).Returns(true);
            _service.DialogHelperStub.Setup(
                helper => helper.Message(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question,
                    It.IsAny<object>())).Returns(messageResult);
            _viewModel.SetIsValid(isValid);
            _viewModel.CloseCommand.Execute(null);
            if (messageResult == null)
                Assert.IsFalse(raised);
            if (messageResult == false)
                Assert.IsTrue(raised);
            if (messageResult == true)
            {
                if (isValid)
                {
                    _contextStub.Verify(c => c.Save(), Times.Once());
                    Assert.IsTrue(raised);
                }
                else
                {
                    _service.DialogHelperStub.Verify(
                        helper => helper.Message(
                            It.IsAny<string>(),
                            It.IsAny<string>(),
                            MessageBoxButton.OK,
                            It.IsAny<MessageBoxImage>(),
                            It.IsAny<object>()));
                    _contextStub.Verify(c => c.Save(), Times.Never());
                    Assert.IsFalse(raised);
                }
            }
        }

        [Test]
        public void CloseCommandExecutesContextDispose()
        {
            _viewModel.Reload();
            _contextStub.Verify(c => c.Dispose(), Times.Never());
            _viewModel.CloseCommand.Execute(null);
            _contextStub.Verify(c => c.Dispose(), Times.Once());
        }

        [Test]
        public void CloseCommandDoesNotExecuteContextDisposeIfClosedWasNotRaised()
        {
            _viewModel.Reload();
            _contextStub.Setup(c => c.HasChanges).Returns(true);
            _service.DialogHelperStub.Setup(
                helper => helper.Message(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question,
                    It.IsAny<object>())).Returns((bool?)null);
            _viewModel.CloseCommand.Execute(null);
            _contextStub.Verify(c => c.Dispose(), Times.Never());
        }

        #endregion

        #region IsValid

        private static readonly object[] ErrorTestCases = new object[] {"Error", null, string.Empty};

        [Test, TestCaseSource("ErrorTestCases")]
        public void IsValidReturnsErrorIsEmpty(string error)
        {
            _viewModel.ErrorMessage = error;
            _viewModel.SetIsValid(true);
            Assert.AreEqual(string.IsNullOrEmpty(error), _viewModel.IsValid);
        }

        #endregion

        #region Context

        [Test]
        public void ContextIsNullAfterCreate()
        {
            Assert.IsNull(_viewModel.Context);
        }

        #endregion

        #region Reload
        
        [Test]
        public void ReloadCreatesNewContextByFactory()
        {
            _viewModel.Reload();
            Assert.AreEqual(_contextStub.Object, _viewModel.Context);
            _service.ContextFactoryStub.Verify();
        }

        [Test]
        public void ReloadAsksYesNolIfContextHasChanges()
        {
            _viewModel.Reload();
            _contextStub.Setup(c => c.HasChanges).Returns(true);
            _viewModel.Reload();
            _service.DialogHelperStub.Verify(
                d => d.Message(
                    It.IsAny<string>(), _viewModel.Name, MessageBoxButton.YesNo, MessageBoxImage.Question, _viewModel.View));
            _contextStub.Verify();
        }

        [Test, TestCaseSource("BooleanTestCases")]
        public void ReloadWhenContextHasChangesTest(bool messageResult)
        {
            _viewModel.Reload();
            _contextStub.Setup(c => c.HasChanges).Returns(true);
            _service.DialogHelperStub.Setup(
                d => d.Message(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(), 
                    It.IsAny<MessageBoxImage>(),
                    It.IsAny<object>()))
                    .Returns(messageResult);
            var result = _viewModel.Reload();
            _service.ContextFactoryStub.Verify(f => f.Create(_viewModel.ConnectionString), Times.Exactly(messageResult ? 2 : 1));
            Assert.AreEqual(messageResult, result);
        }

        [Test]
        public void ReloadExecutesContextDispose()
        {
            _viewModel.Reload();
            _contextStub.Verify(c => c.Dispose(), Times.Never());
            _viewModel.Reload();
            _contextStub.Verify(c => c.Dispose(), Times.Once());
        }

        [Test]
        public void ReloadDoesNotExecuteContextDisposeIfReloadWasNotPerformed()
        {
            _viewModel.Reload();
            _contextStub.Setup(c => c.HasChanges).Returns(true);
            _service.DialogHelperStub.Setup(
                d => d.Message(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(),
                    It.IsAny<MessageBoxImage>(),
                    It.IsAny<object>()))
                    .Returns(false);
            _viewModel.Reload();
            _contextStub.Verify(c => c.Dispose(), Times.Never());
        }

        [Test]
        public void ReloadRaisesPropertyChangedHasChanges()
        {
            var raised = false;
            _viewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "HasChanges")
                        raised = true;
                };
            _viewModel.Reload();
            Assert.IsTrue(raised);
        }

        [Test]
        public void ReloadRaisesSaveCommandCanExecuteChanged()
        {
            var raised = false;
            _viewModel.SaveCommand.CanExecuteChanged += (sender, args) =>
            {
                raised = true;
            };
            _viewModel.Reload();
            Assert.IsTrue(raised);
        }

        [Test]
        public void ReloadShowsExceptionDialogIfExceptionOccured()
        {
            _viewModel.ThrowOnPostReloadAction = true;
            _viewModel.Reload();
            _service.DialogHelperStub.Verify(h => h.Exception(It.IsAny<Exception>(), It.IsAny<object>()));
        }

        [Test]
        public void ReloadRaisesCloseEventIfExceptionOccured()
        {
            var raised = false;
            _viewModel.Closed += (sender, args) =>
            {
                raised = true;
            };
            _viewModel.ThrowOnPostReloadAction = true;
            _viewModel.Reload();
            Assert.IsTrue(raised);
        }

        #endregion

        #region HasChanges

        [Test]
        public void IfContextIsNullHasChangesReturnsFalse()
        {
            Assert.IsNull(_viewModel.Context);
            Assert.IsFalse(_viewModel.HasChanges);
        }

        [Test, TestCaseSource("BooleanTestCases")]
        public void HasChangesReturnsContextHasChanges(bool hasChanges)
        {
            _viewModel.Reload();
            _contextStub.Setup(c => c.HasChanges).Returns(hasChanges);
            Assert.AreEqual(hasChanges, _viewModel.HasChanges);
            _contextStub.Verify();
        }

        [Test]
        public void ContextPropertyChangedRaisesHasChangesPropertyChanged()
        {
            _viewModel.Reload();
            var raised = false;
            _viewModel.PropertyChanged += (sender, args) => { if (args.PropertyName == "HasChanges") raised = true; };
            _contextStub.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs("HasChanges"));
            Assert.IsTrue(raised);
        }

        #endregion

        #region Save

        [Test]
        public void SaveExecutesContextSaveIfIsValid()
        {
            _viewModel.Reload();
            _viewModel.SetIsValid(true);
            Assert.IsTrue(_viewModel.Save());
            _contextStub.Verify(c => c.Save(), Times.Once());
        }

        [Test]
        public void SaveDoesNotExecuteContextSaveIfIsValidReturnsFalse()
        {
            _viewModel.SetIsValid(false);
            Assert.IsFalse(_viewModel.Save());
            _contextStub.Verify(c => c.Save(), Times.Never());
        }

        [Test]
        public void SaveDoesNotExecuteContextSaveIfErrorIsNotEmpty()
        {
            _viewModel.SetIsValid(true);
            _viewModel.ErrorMessage = "Error";
            Assert.IsFalse(_viewModel.Save());
            _contextStub.Verify(c => c.Save(), Times.Never());
        }

        [Test]
        public void SavePromptsIfIsValidReturnsFalse()
        {
            _viewModel.SetIsValid(false);
            _viewModel.Save();
            _service.DialogHelperStub.Verify(
                h => h.Message(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, It.IsAny<object>()));
        }

        [Test]
        public void SavePromptsIfIsErrorIsNotEmptyFalse()
        {
            _viewModel.SetIsValid(true);
            const string msg = "ErrorMessage!";
            _viewModel.ErrorMessage = msg;
            _viewModel.Save();
            _service.DialogHelperStub.Verify(h => h.Message(
                msg, It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, It.IsAny<object>()));
        }

        [Test]
        public void SaveShowsExceptionDialogIfExceptionOccured()
        {
            _viewModel.SetIsValid(true);
            _contextStub.Setup(c => c.Save()).Throws(new Exception("SaveException"));
            _viewModel.Save();
            _contextStub.Verify();
            _service.DialogHelperStub.Verify(h => h.Exception(It.IsAny<Exception>(), It.IsAny<object>()));
        }

        [Test, TestCaseSource("BooleanTestCases")]
        public void SaveCommandCanExecuteTest(bool hasChanges)
        {
            _viewModel.Reload();
            _contextStub.Setup(c => c.HasChanges).Returns(hasChanges);
            Assert.AreEqual(hasChanges, _viewModel.SaveCommand.CanExecute(null));
        }

        [Test]
        public void SaveCommandCanExecuteChangedRaisesWhenContextPropertyChangedRaises()
        {
            _viewModel.Reload();
            var raised = false;
            _viewModel.SaveCommand.CanExecuteChanged += (sender, args) => { raised = true; };
            _contextStub.Raise(c => c.PropertyChanged += null, new PropertyChangedEventArgs("HasChanges"));
            Assert.IsTrue(raised);
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

        private readonly int[] _intValues = { int.MinValue, int.MaxValue, 0 };
        
        private readonly bool[] _booleanCases = { true, false };

        #endregion
    }
}
