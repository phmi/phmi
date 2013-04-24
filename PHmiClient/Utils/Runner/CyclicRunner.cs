using System;
using PHmiClient.Loc;
using PHmiClient.Utils.Notifications;

namespace PHmiClient.Utils.Runner
{
    public class CyclicRunner : ICyclicRunner
    {
        private readonly ITimerService _timerService;
        private readonly INotificationReporter _notificationReporter;
        private readonly IRunTarget _target; 

        public CyclicRunner(ITimerService timerService, IRunTarget target)
        {
            _timerService = timerService;
            _notificationReporter = target.Reporter;
            _target = target;
            _timerService.Elapsed += (sender, args) => Run();
        }

        private void Run()
        {
            try
            {
                Target.Run();
            }
            catch (Exception exception)
            {
                _notificationReporter.Report(Target.Name + ": " + Res.RunError, exception);
                Clean();
            }
        }

        private void Clean()
        {
            try
            {
                Target.Clean();
            }
            catch (Exception exception)
            {
                _notificationReporter.Report(Target.Name + ": " + Res.CleanError, exception);
            }
            
        }

        public void Start()
        {
            _timerService.Start();
        }

        public void Stop()
        {
            _timerService.Stop();
        }

        public TimeSpan TimeSpan
        {
            get { return _timerService.TimeSpan; }
            set { _timerService.TimeSpan = value; }
        }

        public IRunTarget Target { get { return _target; } }
    }
}
