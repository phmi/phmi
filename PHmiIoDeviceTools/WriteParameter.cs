using System;

namespace PHmiIoDeviceTools
{
    [Serializable]
    public class WriteParameter
    {
        private readonly string _address;
        private readonly object _value;

        public WriteParameter(string address, object value)
        {
            _address = address;
            _value = value;
        }

        public string Address
        {
            get { return _address; }
        }

        public object Value
        {
            get { return _value; }
        }
    }
}
