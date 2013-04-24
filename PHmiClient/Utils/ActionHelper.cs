using System;
using System.Windows.Threading;

namespace PHmiClient.Utils
{
    public class ActionHelper : IActionHelper
    {
        private readonly DispatcherSynchronizationContext _context = new DispatcherSynchronizationContext();

        public void Async(Action action)
        {
            action.BeginInvoke(action.EndInvoke, null);
        }

        public void Dispatch(Action action)
        {
            _context.Send(state => action.Invoke(), null);
        }

        public void DispatchAsync(Action action)
        {
            _context.Post(state => action.Invoke(), null);
        }
    }
}
