using PHmiClient.Utils.Notifications;
using PHmiTools.Utils.Npg;

namespace PHmiRunner.Utils
{
    public class DataDbCreatorFactory : IDataDbCreatorFactory
    {
        public IDataDbCreator Create(string connectionString, IReporter reporter)
        {
            return new DataDbCreator(connectionString, new NpgHelper(), reporter);
        }
    }
}
