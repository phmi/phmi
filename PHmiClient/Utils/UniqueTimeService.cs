using System;

namespace PHmiClient.Utils
{
    public class UniqueTimeService
    {
        private readonly ITimeService _timeService;
        private long _prevTicks;

        public UniqueTimeService(ITimeService timeService)
        {
            _timeService = timeService;
        }

        public DateTime UtcTime
        {
            get
            {
                lock (_timeService)
                {
                    var timeTicks = _timeService.UtcTime.Ticks;
                    if (timeTicks <= _prevTicks)
                    {
                        timeTicks = _prevTicks + 1;
                    }
                    _prevTicks = timeTicks;
                    return new DateTime(timeTicks);
                }
            }
        }
    }
}
