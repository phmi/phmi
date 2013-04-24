using PHmiIoDeviceTools;

namespace PHmiIoDevice.Modbus.WriteInfos
{
    internal struct CoilWriteInfo
    {
        public ushort Address;
        public WriteParameter WriteParameter;
        public int Index;
    }
}
