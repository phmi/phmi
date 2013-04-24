using System;
using System.Linq;
using PHmiModel;
using PHmiTools.Utils.Npg;

namespace PHmiTools.Dialogs.Project
{
    public class NewProjectDialogViewModel : ProjectDialogViewModel
    {
        public NewProjectDialogViewModel() : this(null) { }

        public NewProjectDialogViewModel(INewProjectDialogService service) : base(service)
        {
            ConnectionParameters.Database = null;
            _service = service ?? new NewProjectDialogService();
        }

        private readonly INewProjectDialogService _service;
        
        protected override void OkCommandExecuted(object obj)
        {
            InProgress = true;
            ProgressIsIndeterminate = true;
            var script =
                _service.ScriptHelper.ExtractScriptLines(PHmiModelContext.GetPHmiScriptStream())
                .Concat(_service.ScriptHelper.ExtractScriptLines(PHmiModelContext.GetPHmiScriptRowsStream())
                .Select(s => s.Replace("1234567890", Guid.NewGuid().ToString()))
                .Select(s => s.Replace("0987654321", Guid.NewGuid().ToString())))
                .ToArray();
            Progress = 0;
            ProgressMax = script.Length;
            _service.ActionHelper.Async(() =>
                {
                    try
                    {
                        _service.NpgHelper.CreateDatabase(ConnectionParameters);
                        _service.ActionHelper.Dispatch(() => { ProgressIsIndeterminate = false; });
                        _service.NpgHelper.ExecuteScript(
                            ConnectionParameters.ConnectionString,
                            script.Select(r => new NpgQuery(r)).ToArray(),
                            true,
                            row => _service.ActionHelper.Dispatch(() => { Progress = row; }));
                        _service.ActionHelper.Dispatch(() =>
                            {
                                Progress = ProgressMax;
                                base.OkCommandExecuted(obj);
                            });
                    }
                    catch (Exception exception)
                    {
                        ExceptionDialog.Show(exception, View);
                    }
                    finally
                    {
                        _service.ActionHelper.Dispatch(() =>
                            {
                                InProgress = false;
                                ProgressIsIndeterminate = false;
                            });
                    }
                });
        }
    }
}
