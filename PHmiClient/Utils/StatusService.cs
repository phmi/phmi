using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace PHmiClient.Utils
{
    public class StatusService : IStatusService
    {
        private string _message;
        private readonly ITimerService _timer;

        internal StatusService(ITimerService timer)
        {
            _timer = timer;
            LifeTime = TimeSpan.FromSeconds(15);
            _timer.Elapsed += TimerElapsed;
        }

        public StatusService() : this(new TimerService()) { }

        private void TimerElapsed(object sender, EventArgs e)
        {
            Message = null;
        }

        public TimeSpan LifeTime
        {
            get { return _timer.TimeSpan; }
            set
            {
                _timer.TimeSpan = value;
                OnPropertyChanged(s => s.LifeTime);
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                _timer.Stop();
                _timer.Start();
                OnPropertyChanged(s => s.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(Expression<Func<StatusService, object>> getPropertyExpression)
        {
            EventHelper.Raise(ref PropertyChanged, this,
                new PropertyChangedEventArgs(PropertyHelper.GetPropertyName(getPropertyExpression)));
        }
    }
}
