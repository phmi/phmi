using System;

namespace PHmiClient.Utils.Runner
{
    public interface ICyclicRunner : IRunner
    {
        TimeSpan TimeSpan { get; set; }
        IRunTarget Target { get; }
    }
}
