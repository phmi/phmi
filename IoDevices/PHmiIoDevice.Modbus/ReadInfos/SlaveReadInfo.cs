using System.Collections.Generic;
using PHmiIoDeviceTools;

namespace PHmiIoDevice.Modbus.ReadInfos
{
    internal class SlaveReadInfo
    {
        public readonly List<ushort> Coils = new List<ushort>();
        public readonly List<ushort> Contacts = new List<ushort>();
        public readonly List<KeyValuePair<ushort, int>> InputRegisters = new List<KeyValuePair<ushort, int>>();
        public readonly List<KeyValuePair<ushort, int>> HoldingRegisters = new List<KeyValuePair<ushort, int>>();
        
        public readonly Dictionary<ReadParameter, ushort> CoilsAddresses = new Dictionary<ReadParameter, ushort>();
        public readonly Dictionary<ReadParameter, ushort>  ContactsAddresses = new Dictionary<ReadParameter, ushort>();
        public readonly Dictionary<ReadParameter, KeyValuePair<ushort, int>> InputRegistersAddresses
            = new Dictionary<ReadParameter, KeyValuePair<ushort, int>>();
        public readonly Dictionary<ReadParameter, KeyValuePair<ushort, int>> HoldingRegistersAddresses
            = new Dictionary<ReadParameter, KeyValuePair<ushort, int>>();
    }
}
