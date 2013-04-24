using PHmiClient.Utils;
using PHmiTools.Utils.Npg;
using PHmiTools.Utils.Npg.ExIm;

namespace PHmiTools.Dialogs.Project
{
    public class ImportProjectDialogService : ProjectDialogService,
        IImportProjectDialogService
    {
        private readonly INpgExImHelper _exImHelper = new NpgExImHelper();

        private readonly INpgHelper _npgHelper = new NpgHelper();

        private readonly IActionHelper _actionHelper = new ActionHelper();

        private readonly INpgScriptHelper _scriptHelper = new NpgScriptHelper();

        public INpgHelper NpgHelper
        {
            get { return _npgHelper; }
        }

        public IActionHelper ActionHelper
        {
            get { return _actionHelper; }
        }

        public INpgScriptHelper ScriptHelper
        {
            get { return _scriptHelper; }
        }

        public INpgExImHelper ExImHelper
        {
            get { return _exImHelper; }
        }
    }
}
