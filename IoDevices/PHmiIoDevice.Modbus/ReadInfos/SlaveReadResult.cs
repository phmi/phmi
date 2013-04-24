using System.Collections.Generic;

namespace PHmiIoDevice.Modbus.ReadInfos
{
    internal class SlaveReadResult
    {
        public Dictionary<ushort, bool> CoilsData;
        public Dictionary<ushort, bool> ContactsData;
        public Dictionary<KeyValuePair<ushort, int>, byte[]> InputRegistersData;
        public Dictionary<KeyValuePair<ushort, int>, byte[]> HoldingRegistersData;
    }
}
