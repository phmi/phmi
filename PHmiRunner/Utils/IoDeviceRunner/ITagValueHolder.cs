namespace PHmiRunner.Utils.IoDeviceRunner
{
    public interface ITagValueHolder
    {
        void Update(object value);
        object GetWriteValue();
        string Address { get; }
    }

    public interface ITagValueHolder<T> : ITagValueHolder
    {
        T GetValue();
        void SetValue(T value);
    }

    public abstract class TagValueHolder<T> : ITagValueHolder<T>
    {
        protected T Value;
        private bool _valueIsSet;

        public void Update(object value)
        {
            if (!_valueIsSet)
            {
                Value = RawToEng(value);
            }
        }

        public object GetWriteValue()
        {
            var value = EngToRaw(Value);
            _valueIsSet = false;
            return value;
        }

        public abstract string Address { get; }

        protected abstract T RawToEng(object value);
        protected abstract object EngToRaw(T value);

        public T GetValue()
        {
            return Value;
        }

        public void SetValue(T value)
        {
            _valueIsSet = true;
            Value = value;
        }

        public void ClearValue()
        {
            _valueIsSet = false;
            Value = default(T);
        }
    }
}
