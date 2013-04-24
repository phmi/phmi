using PHmiClient.Utils;
using PHmiTools.Utils;
using PHmiTools.Utils.Npg;

namespace PHmiTools.Dialogs.Project
{
    public class OpenProjectDialogService : ProjectDialogService, IOpenProjectDialogService
    {
        private readonly INpgHelper _npgHelper = new NpgHelper();

        private readonly IActionHelper _actionHelper = new ActionHelper();

        private readonly IPHmiDatabaseHelper _databaseHelper = new PHmiDatabaseHelper();

        public INpgHelper NpgHelper
        {
            get { return _npgHelper; }
        }

        public IActionHelper ActionHelper
        {
            get { return _actionHelper; }
        }

        public IPHmiDatabaseHelper DatabaseHelper
        {
            get { return _databaseHelper; }
        }
    }
}
