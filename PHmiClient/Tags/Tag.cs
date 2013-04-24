using System;
using System.ComponentModel;
using System.Threading;
using PHmiClient.Utils;

namespace PHmiClient.Tags
{
    public abstract class Tag<T> : TagAbstract<T>
    {
        private readonly IDispatcherService _dispatcherService;
        private readonly int _id;
        private readonly string _name;
        private readonly Func<string> _descriptionGetter;
        private readonly object _writeLockObj = new object();
        private T _value;
        private Int32 _isRead;
        private bool _isWritten;

        internal override int Id { get { return _id; } }

        public override string Name { get { return _name; } }

        public override string Description { get { return _descriptionGetter.Invoke(); } }

        protected Tag(IDispatcherService dispatcherService, int id, string name, Func<string> descriptionGetter)
        {
            _dispatcherService = dispatcherService;
            _id = id;
            _name = name;
            _descriptionGetter = descriptionGetter;
        } 

        public override T Value
        {
            get
            {
                _isRead = 1;
                return _value;
            }
            set
            {
                lock (_writeLockObj)
                {
                    _value = value;
                    _isWritten = true;
                }
                OnPropertyChanged("Value");
            }
        }

        internal override void UpdateValue(T value)
        {
            lock (_writeLockObj)
            {
                if (!_isWritten)
                    _value = value;
            }
            _dispatcherService.Invoke(() => OnPropertyChanged("Value"));
        }

        internal override bool IsWritten { get { return _isWritten; } }

        internal override T GetWrittenValue()
        {
            lock (_writeLockObj)
            {
                _isWritten = false;
                return _value;
            }
        }

        internal override bool IsRead
        {
            get
            {
                return Interlocked.Exchange(ref _isRead, 0) == 1;
            }
        }

        public override event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string property)
        {
            EventHelper.Raise(ref PropertyChanged, this, new PropertyChangedEventArgs(property));
        }
    }
}
