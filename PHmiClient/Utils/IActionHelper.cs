using System;

namespace PHmiClient.Utils
{
    public interface IActionHelper
    {
        void Async(Action action);
        void Dispatch(Action action);
        void DispatchAsync(Action action);
    }
}
