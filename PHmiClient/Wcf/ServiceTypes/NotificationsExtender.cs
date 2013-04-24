using System.Collections.Generic;
using PHmiClient.Utils.Notifications;

namespace PHmiClient.Wcf.ServiceTypes
{
    internal static class NotificationsExtender
    {
        public static void Report(this IReporter reporter, IEnumerable<WcfNotification> wcfNotifications)
        {
            foreach (var notification in wcfNotifications)
            {
                reporter.Report(
                    notification.StartTime.ToLocalTime() + " " + notification.Message,
                    notification.ShortDescription,
                    notification.LongDescription);
            }
        }
    }
}