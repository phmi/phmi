using System;
using System.ComponentModel;
using PHmiClient.Users;
using PHmiClient.Utils.Pagination;

namespace PHmiClient.Alarms
{
    public abstract class AlarmCategoryAbstract : INotifyPropertyChanged
    {
        internal abstract void SetIdentityGetter(Func<Identity> getter);

        internal abstract int Id { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }

        protected abstract void AddAlarmInfo(int id, Func<string> locationGetter, Func<string> descriptionGetter);

        internal abstract bool HasAlarm(int id);

        internal abstract Tuple<Func<string>, Func<string>> GetAlarmInfo(int id);

        public abstract bool? HasActive { get; internal set; }

        public abstract bool? HasUnacknowledged { get; internal set; }

        public abstract void GetCurrent(CriteriaType criteriaType, AlarmSampleId criteria, int maxCount, Action<Alarm[]> callback);

        internal abstract Tuple<CriteriaType, AlarmSampleId, int, Action<Alarm[]>>[] QueriesForCurrent();

        public abstract void GetHistory(CriteriaType criteriaType, AlarmSampleId criteria, int maxCount, Action<Alarm[]> callback);

        internal abstract Tuple<CriteriaType, AlarmSampleId, int, Action<Alarm[]>>[] QueriesForHistory();

        internal abstract bool IsRead();

        public abstract void Acknowledge(Alarm[] alarms);

        internal abstract Tuple<AlarmSampleId[], Identity>[] AlarmsToAcknowledge();

        public abstract event PropertyChangedEventHandler PropertyChanged;
    }
}
