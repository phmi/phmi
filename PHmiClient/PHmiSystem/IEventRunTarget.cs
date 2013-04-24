using System;

namespace PHmiClient.PHmiSystem
{
    internal interface IEventRunTarget : IServiceRunTarget
    {
        event EventHandler Runned;
    }
}
