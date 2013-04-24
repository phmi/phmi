using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using PHmiClient.Alarms;
using PHmiClient.Logs;
using PHmiClient.Tags;
using PHmiClient.Trends;
using PHmiClient.Users;
using PHmiClient.Utils.Notifications;
using PHmiClient.Utils.Runner;

namespace PHmiClient.PHmiSystem
{
    public abstract class PHmiAbstract : INotifyPropertyChanged, IRunner
    {
        public abstract string Server { get; }

        public abstract ReadOnlyCollection<IoDeviceAbstract> IoDevices { get; }

        protected internal abstract T AddIoDevice<T>(T ioDevice) where T : IoDeviceAbstract;

        protected internal abstract T AddAlarmCategory<T>(T alarmCategory) where T : AlarmCategoryAbstract;

        protected internal abstract T AddTrendsCategory<T>(T trendsCategory) where T : TrendsCategoryAbstract;

        protected internal abstract LogAbstract AddLog(int id, string name);

        public abstract AlarmCategoryAbstract CommonAlarms { get; }

        public abstract INotificationReporter Reporter { get; }

        public abstract event EventHandler BeforeUpdate;

        public abstract event EventHandler AfterUpdate;

        public abstract event PropertyChangedEventHandler PropertyChanged;

        public abstract DateTime Time { get; }

        public abstract IUsers Users { get; }

        public abstract void Start();

        public abstract void Stop();

        public abstract void RunOnce();
    }
}
