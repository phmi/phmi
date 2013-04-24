using PHmiTools.Utils.Npg;

namespace PHmiTools.Utils
{
    public class PHmiDatabaseHelper : IPHmiDatabaseHelper
    {
        public PHmiDatabaseHelper(INpgHelper npgHelper = null)
        {
            _npgHelper = npgHelper ?? new NpgHelper();
        }

        private readonly INpgHelper _npgHelper;

        public bool IsPHmiDatabase(string connectionString, string database)
        {
            var connectionParameters = new NpgConnectionParameters();
            connectionParameters.Update(connectionString);
            connectionParameters.Database = database;
            var query = new NpgQuery(string.Format("SELECT 1 FROM settings WHERE phmi_guid = '{0}'", PHmiConstants.PHmiGuid));
            var result = _npgHelper.ExecuteScalar(connectionParameters.ConnectionString, query);
            return result != null;
        }

        public bool IsPHmiDatabase(INpgConnectionParameters connectionParameters)
        {
            return IsPHmiDatabase(connectionParameters.ConnectionStringWithoutDatabase, connectionParameters.Database);
        }
    }
}
