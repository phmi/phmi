using PHmiClient.Utils;
using PHmiTools.Utils;
using PHmiTools.Utils.Npg;

namespace PHmiTools.Dialogs.Project
{
    public class ProjectDialogService : IProjectDialogService
    {
        private readonly IDialogHelper _dialogHelper = new DialogHelper();

        private readonly IConnectionStringHelper _connectionStringHelper = new ConnectionStringHelper();

        private readonly INpgConnectionParameters _connectionParameters = new NpgConnectionParameters();

        public IDialogHelper DialogHelper
        {
            get { return _dialogHelper; }
        }

        public IConnectionStringHelper ConnectionStringHelper
        {
            get { return _connectionStringHelper; }
        }

        public INpgConnectionParameters ConnectionParameters
        {
            get { return _connectionParameters; }
        }
    }
}
