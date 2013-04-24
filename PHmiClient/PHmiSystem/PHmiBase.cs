using System;
using System.ComponentModel;
using PHmiClient.Alarms;
using PHmiClient.Loc;
using PHmiClient.Logs;
using PHmiClient.Tags;
using PHmiClient.Trends;
using PHmiClient.Users;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Runner;
using PHmiClient.Wcf;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PHmiClient.PHmiSystem
{
    public class PHmiBase : PHmiAbstract
    {
        private readonly string _server;
        private readonly INotificationReporter _reporter;
        private readonly IServiceClientFactory _clientFactory;
        private readonly IUsers _users;
        private readonly ITagService _tagService;
        private readonly IAlarmService _alarmService;
        private readonly ITrendsService _trendsService;
        private readonly ILogService _logService;
        private readonly AlarmCategoryAbstract _commonAlarmCategory;
        private readonly ICyclicRunnerFactory _cyclicRunnerFactory;
        private readonly ICyclicRunner _cyclicRunner;
        private readonly ITimeService _timeService;
        private readonly ITimerService _timerService;
        private readonly IList<IoDeviceAbstract> _ioDevices = new List<IoDeviceAbstract>();
        private readonly ReadOnlyCollection<IoDeviceAbstract> _readOnlyIoDevices;
        private readonly IPHmiRunTarget _pHmiRunTarget;

        protected PHmiBase(string server, string guid) : this(
            new NotificationReporterFactory(),
            new ServiceClientFactory(server, guid),
            new CyclicRunnerFactory(),
            new PHmiRunTargetFactory(),
            new TimeService(),
            new TimerService(),
            new EventRunTarget(new DispatcherService()),
            new UpdateStatusRunTargetFactory(), 
            new UsersRunTarget(),
            new TagServiceFactory(),
            new AlarmServiceFactory(),
            new TrendsServiceFactory(), 
            new LogService(),
            new EventRunTarget(new DispatcherService()))
        {
            _server = server;
        }

        internal PHmiBase(
            INotificationReporterFactory reporterFactory,
            IServiceClientFactory clientFactory,
            ICyclicRunnerFactory cyclicRunnerFactory,
            IPHmiRunTargetFactory pHmiRunTargetFactory,
            ITimeService timeService,
            ITimerService timerService,
            IEventRunTarget beforeUpdateRunTarget,
            IUpdateStatusRunTargetFactory updateStatusRunTargetFactory,
            IUsersRunTarget usersRunTarget,
            ITagServiceFactory tagServiceFactory,
            IAlarmServiceFactory alarmServiceFactory,
            ITrendsServiceFactory trendsServiceFactory,
            ILogService logService,
            IEventRunTarget afterUpdateRunTarget)
        {
            _timeService = timeService;
            _timerService = timerService;
            _timerService.Elapsed += TimerServiceElapsed;
            _reporter = reporterFactory.Create(timeService);
            _clientFactory = clientFactory;
            beforeUpdateRunTarget.Runned += BeforeUpdateRunTargetRunned;
            var updateStatusRunTarget = updateStatusRunTargetFactory.Create(_timeService);
            _users = new Users.Users(usersRunTarget);
            _tagService = tagServiceFactory.Create(_reporter);
            _alarmService = alarmServiceFactory.Create(_reporter);
            _trendsService = trendsServiceFactory.Create(_reporter);
            _logService = logService;
            _commonAlarmCategory = new AlarmCategoryBase(0, "CommonAlarms", () => Res.CommonAlarmsDescription);
            Add(_commonAlarmCategory);
            _cyclicRunnerFactory = cyclicRunnerFactory;
            afterUpdateRunTarget.Runned += AfterUpdateRunTargetRunned;
            _pHmiRunTarget = pHmiRunTargetFactory.Create(
                _reporter,
                _clientFactory,
                beforeUpdateRunTarget,
                updateStatusRunTarget,
                usersRunTarget,
                _tagService,
                _alarmService,
                _trendsService,
                _logService,
                afterUpdateRunTarget);
            _cyclicRunner = _cyclicRunnerFactory.Create(_pHmiRunTarget);
            _readOnlyIoDevices = new ReadOnlyCollection<IoDeviceAbstract>(_ioDevices);
        }

        public override string Server
        {
            get { return _server; }
        }

        public override ReadOnlyCollection<IoDeviceAbstract> IoDevices
        {
            get { return _readOnlyIoDevices; }
        }

        protected internal override T AddIoDevice<T>(T ioDevice)
        {
            _ioDevices.Add(ioDevice);
            _tagService.Add(ioDevice);
            return ioDevice;
        }

        protected internal override T AddAlarmCategory<T>(T alarmCategory)
        {
            Add(alarmCategory);
            return alarmCategory;
        }

        protected internal override T AddTrendsCategory<T>(T trendsCategory)
        {
            _trendsService.Add(trendsCategory);
            return trendsCategory;
        }

        private void Add(AlarmCategoryAbstract category)
        {
            category.SetIdentityGetter(_users.Identity);
            _alarmService.Add(category);
        }

        protected internal override LogAbstract AddLog(int id, string name)
        {
            var log = new Log(id, name);
            _logService.Add(log);
            return log;
        }

        public override AlarmCategoryAbstract CommonAlarms
        {
            get { return _commonAlarmCategory; }
        }

        public override INotificationReporter Reporter
        {
            get { return _reporter; }
        }

        public override event EventHandler BeforeUpdate;

        private void BeforeUpdateRunTargetRunned(object sender, EventArgs e)
        {
            EventHelper.Raise(ref BeforeUpdate, this, EventArgs.Empty);
        }

        public override event EventHandler AfterUpdate;

        private void AfterUpdateRunTargetRunned(object sender, EventArgs e)
        {
            EventHelper.Raise(ref AfterUpdate, this, EventArgs.Empty);
        }

        public override DateTime Time
        {
            get { return _timeService.UtcTime; }
        }

        public override IUsers Users { get { return _users; } }

        public override void Start()
        {
            _timerService.Start();
            _cyclicRunner.Start();
        }

        public override void Stop()
        {
            _timerService.Stop();
            _cyclicRunner.Stop();
        }

        public override void RunOnce()
        {
            try
            {
                _pHmiRunTarget.Run();
            }
            catch (Exception exception)
            {
                _pHmiRunTarget.Reporter.Report(_pHmiRunTarget.Name + ": " + Res.RunError, exception);
            }
            finally
            {
                _pHmiRunTarget.Clean();
            }
        }

        private void TimerServiceElapsed(object sender, EventArgs eventArgs)
        {
            OnPropertyChanged("Time");
        }

        protected virtual void OnPropertyChanged(string property)
        {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(property));
        }

        public override event PropertyChangedEventHandler PropertyChanged;
    }
}
