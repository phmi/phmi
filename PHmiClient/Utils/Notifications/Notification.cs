using System;
using System.ComponentModel;

namespace PHmiClient.Utils.Notifications
{
    public class Notification : INotifyPropertyChanged
    {
        private readonly DateTime _startTime;
        private DateTime? _endTime;
        private readonly string _message;
        private readonly string _shortDescription;
        private readonly string _longDescription;

        internal Notification(DateTime startTime, string message, string shortDescription, string longDescription)
        {
            _startTime = startTime;
            _message = message;
            _shortDescription = shortDescription;
            _longDescription = longDescription ?? _shortDescription;
        }

        public DateTime StartTime { get { return _startTime; } }

        public DateTime? EndTime
        {
            get { return _endTime; }
            internal set
            {
                _endTime = value;
                OnPropertyChanged(PropertyHelper.GetPropertyName(this, m => m.EndTime));
            }
        }

        public string Message { get { return _message; } }

        public string ShortDescription { get { return _shortDescription; } }

        public string LongDescription
        {
            get
            {
                return _longDescription;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
