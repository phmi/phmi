using System.Collections.Generic;
using PHmiIoDeviceTools;

namespace PHmiIoDevice.Melsec.WriteInfos
{
    internal struct RegisterWriteInfo
    {
        public KeyValuePair<int, int> Address;
        public WriteParameter WriteParameter;
        public int Index;
    }
}
