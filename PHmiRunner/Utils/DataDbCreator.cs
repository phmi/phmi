using PHmiClient.Utils.Notifications;
using PHmiResources.Loc;
using PHmiTools.Utils.Npg;

namespace PHmiRunner.Utils
{
    public class DataDbCreator : IDataDbCreator
    {
        private readonly string _connectionsString;
        private readonly INpgHelper _npgHelper;
        private readonly IReporter _reporter;

        public DataDbCreator(string connectionString, INpgHelper npgHelper, IReporter reporter)
        {
            _connectionsString = connectionString;
            _npgHelper = npgHelper;
            _reporter = reporter;
        }

        public bool Start()
        {
            if (!_npgHelper.DatabaseExists(_connectionsString))
            {
                _npgHelper.CreateDatabase(_connectionsString);
                _reporter.Report(string.Format(Res.DatabaseCreatedMessage, _npgHelper.GetDatabase(_connectionsString)));
                return true;
            }
            return false;
        }
    }
}
