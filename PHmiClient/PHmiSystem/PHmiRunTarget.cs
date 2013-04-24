using System;
using PHmiClient.Utils.Notifications;
using PHmiClient.Loc;
using PHmiClient.Wcf;

namespace PHmiClient.PHmiSystem
{
    internal class PHmiRunTarget : IPHmiRunTarget
    {
        private readonly INotificationReporter _reporter;
        private readonly IServiceClientFactory _clientFactory;
        private readonly IServiceRunTarget[] _targets;
        private IServiceClient _client;

        public PHmiRunTarget(INotificationReporter reporter, IServiceClientFactory clientFactory, params IServiceRunTarget[] targets)
        {
            _reporter = reporter;
            _clientFactory = clientFactory;
            _targets = targets;
        }

        public void Run()
        {
            foreach (var target in _targets)
            {
                Run(target);
            }
        }

        private void Run(IServiceRunTarget target)
        {
            if (_client == null)
                _client = _clientFactory.Create();
            try
            {
                target.Run(_client);
            }
            catch (Exception exception)
            {
                _reporter.Report(string.Format("{0}: {1}", target.Name, Res.RunError), exception);
                Clean(target);
                Clean();
            }
        }

        private void Clean(IServiceRunTarget target)
        {
            try
            {
                target.Clean();
            }
            catch (Exception exception)
            {
                _reporter.Report(string.Format("{0}: {1}", target.Name, Res.CleanError), exception);
            }
        }

        public void Clean()
        {
            try
            {
                if (_client != null)
                    _client.Dispose();
            }
            catch (Exception exception)
            {
                _reporter.Report(string.Format("{0}: {1}", Name, Res.CleanError), exception);
            }
            _client = null;
        }

        public string Name
        {
            get { return Res.PHmi; }
        }

        public INotificationReporter Reporter
        {
            get { return _reporter; }
        }
    }
}
