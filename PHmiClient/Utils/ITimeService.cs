using System;

namespace PHmiClient.Utils
{
    public interface ITimeService
    {
        DateTime UtcTime { get; set; }
    }
}
