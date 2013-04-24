using System;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiModel;
using PHmiResources.Loc;
using PHmiRunner.Utils;
using PHmiService.Utils;
using PHmiTools;
using PHmiTools.Utils.Npg;
using System.ServiceProcess;

namespace PHmiService
{
    public partial class Service : ServiceBase
    {
        private readonly IReporter _reporter;
        private readonly IProjectRunnerFactory _runnerFactory;
        private IProjectRunner _runner;
        
        public Service()
        {
            InitializeComponent();
            eventLog1.Source = PHmiConstants.PHmiServiceName;
            _reporter = new EventLogReporter(eventLog1);
            _runnerFactory = new ProjectRunnerFactory(
                new TimeService(),
                _reporter,
                new PHmiModelContextFactory(),
                new NpgHelper());
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                var connectionStringHelper = new ConnectionStringHelper();
                var connectionString = connectionStringHelper.Get(PHmiConstants.PHmiConnectionStringName);
                connectionStringHelper.Protect();
                var connectionParameters = new NpgConnectionParameters();
                connectionParameters.Update(connectionString);

                _runner = _runnerFactory.Create(connectionParameters.Database, connectionString);
                _runner.Start();
            }
            catch (Exception exception)
            {
                _reporter.Report(Res.StartError, exception);
            }
        }

        protected override void OnStop()
        {
            try
            {
                _runner.Stop();
            }
            catch (Exception exception)
            {
                _reporter.Report(Res.StopError, exception);
            }
            finally
            {
                _runner = null;
            }
        }
    }
}
