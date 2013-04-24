using System;

namespace PHmiClient.Utils.Notifications
{
    public interface IReporter
    {
        void Report(string message, string shortDescription = null, string longDescription = null);
        void Report(string message, Exception exception);
    }
}
