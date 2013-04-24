using System;

namespace PHmiIoDeviceTools
{
    [Serializable]
    public class ReadParameter
    {
        private readonly string _address;
        private readonly Type _valueType;

        public ReadParameter(string address, Type valueType)
        {
            _address = address;
            _valueType = valueType;
        }

        public string Address
        {
            get { return _address; }
        }

        public Type ValueType
        {
            get { return _valueType; }
        }
    }
}
