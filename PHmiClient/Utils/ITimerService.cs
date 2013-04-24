using System;

namespace PHmiClient.Utils
{
    public interface ITimerService
    {
        event EventHandler Elapsed;

        TimeSpan TimeSpan { get; set; }

        void Start();

        void Stop();
    }
}
