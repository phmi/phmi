using PHmiClient.Utils;
using PHmiTools.Utils;
using PHmiTools.Utils.Npg;
using PHmiTools.Utils.Npg.ExIm;

namespace PHmiTools.Dialogs.Project
{
    public class ExportProjectDialogService : IExportProjectDialogService
    {
        private readonly INpgExImHelper _exImHelper = new NpgExImHelper();

        private readonly IActionHelper _actionHelper = new ActionHelper();

        private readonly IDialogHelper _dialogHelper = new DialogHelper();

        private readonly INpgScriptHelper _scriptHelper = new NpgScriptHelper();

        private readonly INpgHelper _npgHelper = new NpgHelper();

        public IActionHelper ActionHelper
        {
            get { return _actionHelper; }
        }

        public INpgExImHelper ExImHelper
        {
            get { return _exImHelper; }
        }

        public IDialogHelper DialogHelper
        {
            get { return _dialogHelper; }
        }

        public INpgScriptHelper ScriptHelper
        {
            get { return _scriptHelper; }
        }

        public INpgHelper NpgHelper
        {
            get { return _npgHelper; }
        }
    }
}
