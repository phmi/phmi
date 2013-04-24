using System.Collections.Generic;

namespace PHmiIoDevice.Modbus.BytesConverters
{
    internal interface IBytesToRegistersBytesConverter
    {
        byte[] GetBytes(IList<byte> message, int startIndex, int registersCount);
    }
}
