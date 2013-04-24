using System;
using System.ComponentModel;
using System.Threading;

namespace PHmiClient.Utils
{
    public static class EventHelper
    {
        public static bool Raise(ref EventHandler eventHandler, object sender, EventArgs args)
        {
            var temp = Interlocked.CompareExchange(ref eventHandler, null, null);
            if (temp != null)
            {
                temp(sender, args);
                return true;
            }
            return false;
        }

        public static bool Raise<T>(ref EventHandler<T> eventHandler, object sender, T args) where T : EventArgs
        {
            var temp = Interlocked.CompareExchange(ref eventHandler, null, null);
            if (temp != null)
            {
                temp(sender, args);
                return true;
            }
            return false;
        }

        public static bool Raise(ref PropertyChangedEventHandler eventHandler, object sender, PropertyChangedEventArgs args)
        {
            var temp = Interlocked.CompareExchange(ref eventHandler, null, null);
            if (temp != null)
            {
                temp(sender, args);
                return true;
            }
            return false;
        }
    }
}
