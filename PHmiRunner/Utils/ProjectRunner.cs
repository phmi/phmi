using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PHmiClient.Utils;
using PHmiClient.Utils.Runner;
using PHmiResources.Loc;
using PHmiClient.Utils.Notifications;
using PHmiModel;
using PHmiModel.Entities;
using PHmiModel.Interfaces;
using PHmiRunner.Utils.Alarms;
using PHmiRunner.Utils.IoDeviceRunner;
using PHmiRunner.Utils.Logs;
using PHmiRunner.Utils.Trends;
using PHmiRunner.Utils.Users;
using PHmiRunner.Utils.Wcf;

namespace PHmiRunner.Utils
{
    public class ProjectRunner : IProjectRunner
    {
        private readonly string _projectName;
        private IModelContext _context;
        private readonly ITimeService _timeService;
        private readonly IReporter _reporter;
        private readonly IDataDbCreator _dataDbCreator;
        private readonly IUsersRunnerFactory _usersRunnerFactory;
        private readonly IServiceRunnerFactory _serviceRunnerFactory;
        private readonly ICyclicRunnerFactory _cyclicRunnerFactory;
        private readonly IIoDeviceRunTargetFactory _ioDeviceRunTargetFactory;
        private readonly IAlarmsRunTargetFactory _alarmsRunTargetFactory;
        private readonly ITrendsRunTargetFactory _trendsRunTargetFactory;
        private readonly ILogRunTargetFactory _logMaintainerFactory;
        private readonly string _dataDbConnectionString;
        private readonly IDictionary<int, IIoDeviceRunTarget> _ioDeviceRunTargets = new Dictionary<int, IIoDeviceRunTarget>();
        private readonly IList<Tuple<IRunner, string, IIoDeviceRunTarget>> _ioDeviceRunners
            = new List<Tuple<IRunner, string, IIoDeviceRunTarget>>();
        private readonly IDictionary<int, IAlarmsRunTarget> _alarmsRunTargets = new Dictionary<int, IAlarmsRunTarget>();
        private readonly IList<Tuple<IRunner, string>> _alarmsRunners = new List<Tuple<IRunner, string>>();
        private readonly IDictionary<int, ITrendsRunTarget> _trendsRunTargets = new Dictionary<int, ITrendsRunTarget>();
        private readonly IList<Tuple<IRunner, string>> _trendsRunners = new List<Tuple<IRunner, string>>(); 
        private readonly IDictionary<int, ILogMaintainer> _logMaintainers = new Dictionary<int, ILogMaintainer>();
        private IRunner _serviceRunner;
        
        public ProjectRunner(
            string projectName,
            IModelContext context,
            ITimeService timeService,
            IReporter reporter,
            string dataDbConnectionString,
            IDataDbCreatorFactory dataDbCreatorFactory,
            IUsersRunnerFactory usersRunnerFactory,
            IServiceRunnerFactory serviceRunnerFactory,
            ICyclicRunnerFactory cyclicRunnerFactory,
            IIoDeviceRunTargetFactory ioDeviceRunTargetFactory,
            IAlarmsRunTargetFactory alarmsRunTargetFactory,
            ITrendsRunTargetFactory trendsRunTargetFactory,
            ILogRunTargetFactory logMaintainerFactory)
        {
            _projectName = projectName;
            _context = context;
            _timeService = timeService;
            _reporter = reporter;
            _dataDbConnectionString = dataDbConnectionString;
            _dataDbCreator = dataDbCreatorFactory.Create(_dataDbConnectionString, _reporter);
            _usersRunnerFactory = usersRunnerFactory;
            _serviceRunnerFactory = serviceRunnerFactory;
            _cyclicRunnerFactory = cyclicRunnerFactory;
            _ioDeviceRunTargetFactory = ioDeviceRunTargetFactory;
            _alarmsRunTargetFactory = alarmsRunTargetFactory;
            _trendsRunTargetFactory = trendsRunTargetFactory;
            _logMaintainerFactory = logMaintainerFactory;
        }

        public void Start()
        {
            try
            {
                _reporter.Report(string.Format(Res.ProjectStartingMessage, _projectName));
                _dataDbCreator.Start();
                StartUsersRunner();
                StartIoDevices();
                StartAlarms();
                StartTrends();
                StartLogs();
                StartService();
                _reporter.Report(string.Format(Res.ProjectStartedMessage, _projectName));
                _context.Dispose();
                _context = null;
            }
            catch (Exception exception)
            {
                _reporter.Report(Res.StartError, exception);
            }
        }

        private void StartUsersRunner()
        {
            UsersRunner = _usersRunnerFactory.Create(_context, _dataDbConnectionString);
            UsersRunner.Start();
            _reporter.Report(Res.UsersServiceStarted);
        }

        private void StartIoDevices()
        {
            foreach (var ioDevice in _context.Get<IoDevice>().ToArray())
            {
                var ioDeviceRunTarget = _ioDeviceRunTargetFactory.Create(_timeService, ioDevice);
                _ioDeviceRunTargets.Add(ioDevice.Id, ioDeviceRunTarget);
                var runner = _cyclicRunnerFactory.Create(ioDeviceRunTarget);
                _ioDeviceRunners.Add(new Tuple<IRunner, string, IIoDeviceRunTarget>(runner, ioDevice.Name, ioDeviceRunTarget));
                runner.Start();
                _reporter.Report(string.Format(Res.IoDeviceStartedMessage, ioDevice.Name));
            }
        }

        private void StartAlarms()
        {
            foreach (var category in _context.Get<AlarmCategory>().ToArray())
            {
                var alarmRunTarget = _alarmsRunTargetFactory.Create(
                    _dataDbConnectionString, this, category, _timeService);
                _alarmsRunTargets.Add(category.Id, alarmRunTarget);
                var runner = _cyclicRunnerFactory.Create(alarmRunTarget);
                _alarmsRunners.Add(new Tuple<IRunner, string>(runner, category.Name));
                runner.Start();
                _reporter.Report(string.Format(Res.AlarmsStartedMessage, category.Name));
            }
        }

        private void StartTrends()
        {
            foreach (var category in _context.Get<TrendCategory>().ToArray())
            {
                var trendRunTarget = _trendsRunTargetFactory.Create(
                    _dataDbConnectionString, this, category, _timeService);
                _trendsRunTargets.Add(category.Id, trendRunTarget);
                var runner = _cyclicRunnerFactory.Create(trendRunTarget);
                runner.TimeSpan = new TimeSpan(category.PeriodDb);
                _trendsRunners.Add(new Tuple<IRunner, string>(runner, category.Name));
                runner.Start();
                _reporter.Report(string.Format(Res.TrendsStartedMessage, category.Name));
            }
        }

        private void StartLogs()
        {
            foreach (var log in _context.Get<PHmiModel.Entities.Log>().ToArray())
            {
                var logRunTarget = _logMaintainerFactory.Create(
                    _dataDbConnectionString, log, _timeService);
                _logMaintainers.Add(log.Id, logRunTarget);
                _reporter.Report(string.Format(Res.LogStargedMessage, log.Name));
            }
        }

        private void StartService()
        {
            var settings = _context.Get<Settings>().Single();
            _serviceRunner = _serviceRunnerFactory.Create(this, settings.Server, settings.Guid, _timeService);
            _serviceRunner.Start();
            _reporter.Report(Res.ServiceIsStarted);
        }

        public void Stop()
        {
            try
            {
                _reporter.Report(string.Format(Res.ProjectStoppingMessage, _projectName));
                StopRunners();
                _reporter.Report(string.Format(Res.ProjectStoppedMessage, _projectName));
            }
            catch (Exception exception)
            {
                _reporter.Report(Res.StopError, exception);
            }
        }

        private void StopRunners()
        {
            StopService();
            StopAlarms();
            StopTrends();
            StopIoDevices();
        }

        private void StopService()
        {
            _serviceRunner.Stop();
            _reporter.Report(Res.ServiceIsStopped);
        }

        private void StopAlarms()
        {
            foreach (var t in _alarmsRunners)
            {
                t.Item1.Stop();
                _reporter.Report(string.Format(Res.AlarmsStoppedMessage, t.Item2));
            }
        }

        private void StopTrends()
        {
            foreach (var t in _trendsRunners)
            {
                t.Item1.Stop();
                _reporter.Report(string.Format(Res.TrendsStoppedMessage, t.Item2));
            }
        }

        private void StopIoDevices()
        {
            foreach (var tuple in _ioDeviceRunners)
            {
                tuple.Item1.Stop();
                tuple.Item3.Dispose();
                _reporter.Report(string.Format(Res.IoDeviceStoppedMessage, tuple.Item2));
            }
        }

        public IDictionary<int, IIoDeviceRunTarget> IoDeviceRunTargets { get { return _ioDeviceRunTargets; } }

        public IUsersRunner UsersRunner { get; private set; }

        public IDictionary<int, IAlarmsRunTarget> AlarmsRunTargets { get { return _alarmsRunTargets; } }

        public IDictionary<int, ITrendsRunTarget> TrendsRunTargets { get { return _trendsRunTargets; } }

        public IDictionary<int, ILogMaintainer> LogMaintainers { get { return _logMaintainers; } }
    }
}
