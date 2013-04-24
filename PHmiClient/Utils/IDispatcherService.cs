using System;

namespace PHmiClient.Utils
{
    public interface IDispatcherService
    {
        void Invoke(Action action);
    }
}
