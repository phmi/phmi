using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace PHmiClient.Utils.Notifications
{
    public class NotificationReporter : INotificationReporter
    {
        private class Key : Tuple<string, string, string>
        {
            public Key(string message, string shortDescription, string longDescription) : base(message, shortDescription, longDescription)
            {
            }
        }

        private class NotificationWithLastTime
        {
            public readonly Notification Notification;
            public DateTime LastTime;

            public NotificationWithLastTime(Notification notification, DateTime lastTime)
            {
                Notification = notification;
                LastTime = lastTime;
            }
        }

        private readonly ObservableCollection<Notification> _notificationList = new ObservableCollection<Notification>();
        private readonly ReadOnlyObservableCollection<Notification> _notificationObs;
        private readonly UniqueTimeService _timeService;
        private readonly ITimerService _timerService;
        private readonly IDictionary<Key, NotificationWithLastTime> _notificationDic = new Dictionary<Key, NotificationWithLastTime>();
        private readonly IDispatcherService _dispatcherService = new DispatcherService();
        private readonly ISet<Notification> _expiredNotifications = new HashSet<Notification>();
        private bool _containsActiveNotifications;
        
        internal NotificationReporter(ITimeService timeService, ITimerService timerService)
        {
            _notificationList = new ObservableCollection<Notification>();
            _notificationObs = new ReadOnlyObservableCollection<Notification>(_notificationList);
            _timeService = new UniqueTimeService(timeService);
            _timerService = timerService;
            _timerService.TimeSpan = TimeSpan.FromSeconds(1);
            ExpirationTime = TimeSpan.FromSeconds(15);
            LifeTime = TimeSpan.FromDays(7);
            _timerService.Elapsed += TimerServiceElapsed;
            _timerService.Start();
        }

        public NotificationReporter(ITimeService timeService) : this(timeService, new TimerService()) { }

        private void TimerServiceElapsed(object sender, EventArgs e)
        {
            var now = _timeService.UtcTime;
            foreach (var item in _notificationDic.ToArray().Where(item => now > item.Value.LastTime + ExpirationTime))
            {
                item.Value.Notification.EndTime = item.Value.LastTime;
                _notificationDic.Remove(item);
                _expiredNotifications.Add(item.Value.Notification);
            }
            ContainsActiveNotifications = _notificationDic.Any();
            foreach (var msg in _expiredNotifications.ToArray().Where(msg => now > msg.EndTime + LifeTime))
            {
                Reset(msg);
            }
        }

        public TimeSpan ExpirationTime { get; set; }

        public TimeSpan LifeTime { get; set; }

        public ReadOnlyObservableCollection<Notification> Notifications
        {
            get { return _notificationObs; }
        }

        public void Report(string message, string shortDescription = null, string longDescription = null)
        {
            var key = new Key(message, shortDescription, longDescription);
            var value = GetValue(key);
            var currentTime = _timeService.UtcTime;
            if (value == null)
            {
                var notification = new Notification(currentTime, message, shortDescription, longDescription);
                value = new NotificationWithLastTime(notification, currentTime);
                AddValue(key, value);
            }
            else
            {
                value.LastTime = currentTime;
            }
            ContainsActiveNotifications = true;
        }

        public void Report(string message, Exception exception)
        {
            Report(message, exception.Message, exception.ToString());
        }

        private NotificationWithLastTime GetValue(Key key)
        {
            NotificationWithLastTime tuple;
            return _notificationDic.TryGetValue(key, out tuple) ? tuple : null;
        }

        private void AddValue(Key key, NotificationWithLastTime item)
        {
            _notificationDic.Add(key, item);
            _dispatcherService.Invoke(() => _notificationList.Add(item.Notification));
        }

        public void Reset(Notification notification)
        {
            lock (_expiredNotifications)
            {
                if (_expiredNotifications.Contains(notification))
                {
                    _expiredNotifications.Remove(notification);
                    _dispatcherService.Invoke(() => _notificationList.Remove(notification));
                }
            }
        }

        public void ResetAll()
        {
            foreach (var notification in _expiredNotifications.ToArray())
            {
                Reset(notification);
            }
        }

        public bool ContainsActiveNotifications
        {
            get { return _containsActiveNotifications; }
            private set
            {
                if (_containsActiveNotifications == value)
                    return;
                _containsActiveNotifications = value;
                OnPropertyChanged("ContainsActiveNotifications");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string property)
        {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(property));
        }
    }
}
