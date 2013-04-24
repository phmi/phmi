using System;
using System.Timers;

namespace PHmiClient.Utils
{
    public class TimerService : ITimerService
    {
        private readonly Timer _timer = new Timer();
        private bool _running;

        public TimerService()
        {
            _timer.Interval = 100;
            _timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            EventHelper.Raise(ref Elapsed, this, EventArgs.Empty);
            lock (_timer)
            {
                if (_running)
                {
                    _timer.Start();
                }
            }
        }

        public event EventHandler Elapsed;

        public TimeSpan TimeSpan
        {
            get { return TimeSpan.FromMilliseconds(_timer.Interval); }
            set { _timer.Interval = value.TotalMilliseconds; }
        }

        public void Start()
        {
            lock (_timer)
            {
                _running = true;
                _timer.Start();
            }
        }

        public void Stop()
        {
            lock (_timer)
            {
                _running = false;
                _timer.Stop();
            }
        }
    }
}
