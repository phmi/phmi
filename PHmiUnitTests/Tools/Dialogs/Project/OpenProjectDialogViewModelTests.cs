using System;
using Moq;
using NUnit.Framework;
using PHmiClient.Utils;
using PHmiClient.Utils.ViewInterfaces;
using PHmiTools.Dialogs.Project;
using PHmiTools.Utils;
using PHmiTools.Utils.Npg;

namespace PHmiUnitTests.Tools.Dialogs.Project
{
    [TestFixture]
    public class OpenProjectDialogViewModelTests
    {
        #region Stubs

        public class Service : ProjectDialogViewModelTests.Service, IOpenProjectDialogService
        {
            public readonly Mock<INpgHelper> NpgHelperStub = new Mock<INpgHelper>();

            private readonly IActionHelper _actionHelper = new ActionHelperStub();

            public readonly Mock<IPHmiDatabaseHelper> DatabaseHelperStub = new Mock<IPHmiDatabaseHelper>();

            public INpgHelper NpgHelper
            {
                get { return NpgHelperStub.Object; }
            }

            public IActionHelper ActionHelper
            {
                get { return _actionHelper; }
            }

            public IPHmiDatabaseHelper DatabaseHelper
            {
                get { return DatabaseHelperStub.Object; }
            }
        }
        
        private class ActionHelperStub : IActionHelper
        {
            public void Async(Action action)
            {
                action.Invoke();
            }

            public void Dispatch(Action action)
            {
                action.Invoke();
            }

            public void DispatchAsync(Action action)
            {
                throw new NotImplementedException();
            }
        }

        private Service _service;
        private Mock<IWindow> _windowStub;
        private OpenProjectDialogViewModel _viewModel;

        #endregion

        [SetUp]
        public void SetUp()
        {
            _service = new Service();
            _windowStub = new Mock<IWindow>();
            _viewModel = new OpenProjectDialogViewModel(_service) {View = _windowStub.Object};
        }

        [Test]
        public void OkCommandExecuted()
        {
            _service.DatabaseHelperStub.Setup(helper => helper.IsPHmiDatabase(_service.ConnectionParameters)).Returns(true);
            _viewModel.OkCommand.Execute(null);
            _service.DatabaseHelperStub.Verify();
            _windowStub.VerifySet(w => w.DialogResult = true, Times.Once());
        }

        [Test]
        public void LoadDatabasesCommandExecuted()
        {
            _service.NpgHelperStub.Setup(h => h.GetDatabases(_viewModel.ConnectionParameters))
                .Returns(new [] {"database1", "database2", "database3"});
            _service.DatabaseHelperStub
                .Setup(helper => helper.IsPHmiDatabase(
                    _viewModel.ConnectionParameters.ConnectionStringWithoutDatabase, "database1"))
                .Returns(true);
            _service.DatabaseHelperStub
                .Setup(helper => helper.IsPHmiDatabase(
                    _viewModel.ConnectionParameters.ConnectionStringWithoutDatabase, "database2"))
                .Returns(false);
            _service.DatabaseHelperStub
                .Setup(helper => helper.IsPHmiDatabase(
                    _viewModel.ConnectionParameters.ConnectionStringWithoutDatabase, "database3"))
                .Returns(true);
            _viewModel.LoadDatabasesCommand.Execute(null);
            _service.NpgHelperStub.Verify();
            _service.DatabaseHelperStub.Verify();
            Assert.AreEqual(2, _viewModel.Databases.Count);
            Assert.AreEqual("database1", _viewModel.Databases[0]);
            Assert.AreEqual("database3", _viewModel.Databases[1]);
        }
    }
}
