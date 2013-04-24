using PHmiClient.Utils.Notifications;
using System;
using System.Diagnostics;

namespace PHmiService.Utils
{
    public class EventLogReporter : IReporter
    {
        private readonly EventLog _eventLog;

        public EventLogReporter(EventLog eventLog)
        {
            _eventLog = eventLog;
        }

        public void Report(DateTime startTime, string message, string shortDescription = null, string longDescription = null)
        {
            Report(message, shortDescription, longDescription);
        }

        public void Report(string message, string shortDescription = null, string longDescription = null)
        {
            var description = longDescription ?? shortDescription;
            var log = message;
            if (!string.IsNullOrEmpty(description))
            {
                log += Environment.NewLine + description;
            }
            AddLog(log);
        }

        public void Report(string message, Exception exception)
        {
            Report(message, exception.ToString());
        }

        private void AddLog(string log)
        {
            _eventLog.WriteEntry(log);
        }
    }
}
