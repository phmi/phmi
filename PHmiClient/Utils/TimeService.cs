using System;

namespace PHmiClient.Utils
{
    public class TimeService : ITimeService
    {
        private TimeSpan _offset;

        public DateTime UtcTime
        {
            get { return DateTime.UtcNow + _offset; }
            set { _offset = value - DateTime.UtcNow; }
        }
    }
}
