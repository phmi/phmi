using System.Collections.Generic;
using PHmiIoDeviceTools;

namespace PHmiIoDevice.Modbus.WriteInfos
{
    internal struct HoldingRegisterWriteInfo
    {
        public KeyValuePair<ushort, int> Address;
        public WriteParameter WriteParameter;
        public int Index;
    }
}
