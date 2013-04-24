using System;
using System.Windows;

namespace PHmiClient.Utils
{
    public class DispatcherService : IDispatcherService
    {
        public void Invoke(Action action)
        {
            var app = Application.Current;
            if (app == null)
            {
                action.Invoke();
                return;
            }
            var dispatcher = app.Dispatcher;
            if (dispatcher == null)
            {
                action.Invoke();
                return;
            }
            if (dispatcher.CheckAccess())
            {
                action.Invoke();
                return;
            }
            dispatcher.Invoke(action);
        }
    }
}
