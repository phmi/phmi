using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PHmiClient.Utils.Notifications
{
    public interface INotificationReporter : IReporter, INotifyPropertyChanged
    {
        ReadOnlyObservableCollection<Notification> Notifications { get; }
        TimeSpan ExpirationTime { get; set; }
        TimeSpan LifeTime { get; set; }
        void Reset(Notification notification);
        void ResetAll();
        bool ContainsActiveNotifications { get; }
    }
}
