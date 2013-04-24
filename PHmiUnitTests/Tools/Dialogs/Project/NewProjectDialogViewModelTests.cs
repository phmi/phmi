using System;
using System.IO;
using Moq;
using NUnit.Framework;
using PHmiClient.Utils;
using PHmiClient.Utils.ViewInterfaces;
using PHmiTools.Dialogs.Project;
using PHmiTools.Utils.Npg;

namespace PHmiUnitTests.Tools.Dialogs.Project
{
    [TestFixture]
    public class NewProjectDialogViewModelTests
    {
        #region Stubs

        public class Service : ProjectDialogViewModelTests.Service, INewProjectDialogService
        {
            public readonly Mock<INpgHelper> NpgHelperStub = new Mock<INpgHelper>();

            private readonly IActionHelper _actionHelper = new ActionHelperStub();

            public readonly Mock<INpgScriptHelper> ScriptHelperStub = new Mock<INpgScriptHelper>();

            public INpgHelper NpgHelper
            {
                get { return NpgHelperStub.Object; }
            }

            public IActionHelper ActionHelper
            {
                get { return _actionHelper; }
            }

            public INpgScriptHelper ScriptHelper
            {
                get { return ScriptHelperStub.Object; }
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
        private NewProjectDialogViewModel _viewModel;

        #endregion

        private void CreateViewModel()
        {
            _viewModel = new NewProjectDialogViewModel(_service) {View = _windowStub.Object};
        }

        [SetUp]
        public void SetUp()
        {
            _service = new Service();
            _windowStub = new Mock<IWindow>();
            CreateViewModel();
        }

        [Test]
        public void RemovesDatabaseIfConnectionStringIsNotEmpty()
        {
            var parameters = new NpgConnectionParameters { Database = "database" };
            _service.ConnectionStringHelperStub
                .Setup(helper => helper.Get(It.IsAny<string>()))
                .Returns(parameters.ConnectionString);
            CreateViewModel();
            Assert.IsNull(_viewModel.ConnectionParameters.Database);
        }
        
        #region OkCommand

        [Test]
        public void OkCommandExecuted()
        {
            _service.ScriptHelperStub
                .Setup(helper => helper.ExtractScriptLines(It.IsAny<Stream>()))
                .Returns(new[] {"line1", "line2"});
            _viewModel.OkCommand.Execute(null);
            _service.ScriptHelperStub.Verify();
            _service.NpgHelperStub.Verify(helper => helper.CreateDatabase(_service.ConnectionParameters));
            _service.NpgHelperStub.Verify(helper => 
                helper.ExecuteScript(
                _service.ConnectionParameters.ConnectionString,
                It.Is<NpgQuery[]>(items => items.Length == 4 && items[0].Text == "line1" && items[1].Text == "line2"),
                true,
                It.IsAny<Action<int>>()));
            _windowStub.VerifySet(w => w.DialogResult = true, Times.Once());
        }

        #endregion
    }
}
