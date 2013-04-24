using PHmiClient.Utils;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace PHmiClient.Alarms
{
    [DataContract]
    public sealed class Alarm : INotifyPropertyChanged
    {
        private DateTime? _endTime;
        private DateTime? _acknowledgeTime;
        [field: NonSerialized] private Func<string> _locationGetter;
        [field: NonSerialized] private Func<string> _descriptionGetter;
        private long? _userId;
        [NonSerialized]
        private AlarmCategoryAbstract _category;

        public Alarm(DateTime startTime, int alarmId)
        {
            StartTime = startTime;
            AlarmId = alarmId;
        }

        [DataMember]
        public DateTime StartTime { get; private set; }

        [DataMember]
        public int AlarmId { get; private set; }

        internal void SetAlarmInfo(Func<string> locationGetter, Func<string> descriptionGetter, AlarmCategoryAbstract category)
        {
            _locationGetter = locationGetter;
            _descriptionGetter = descriptionGetter;
            _category = category;
        }

        public string Location { get { return _locationGetter(); } }

        public string Description { get { return _descriptionGetter(); } }

        public AlarmCategoryAbstract Category { get { return _category; } }

        [DataMember]
        public DateTime? EndTime
        {
            get { return _endTime; }
            internal set
            {
                _endTime = value;
                OnPropertyChanged("EndTime");
            }
        }

        [DataMember]
        public DateTime? AcknowledgeTime
        {
            get { return _acknowledgeTime; }
            internal set
            {
                _acknowledgeTime = value;
                OnPropertyChanged("AcknowledgeTime");
            }
        }

        [DataMember]
        public long? UserId
        {
            get { return _userId; }
            internal set
            {
                _userId = value;
                OnPropertyChanged("UserId");
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateFrom(Alarm alarm)
        {
            EndTime = alarm.EndTime;
            AcknowledgeTime = alarm.AcknowledgeTime;
            UserId = alarm.UserId;
        }
    }
}
