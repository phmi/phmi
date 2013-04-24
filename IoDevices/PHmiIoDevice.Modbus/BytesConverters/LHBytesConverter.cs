using System.Collections.Generic;

namespace PHmiIoDevice.Modbus.BytesConverters
{
    internal class LHBytesConverter : IBytesToRegistersBytesConverter
    {
        public byte[] GetBytes(IList<byte> message, int startIndex, int registersCount)
        {
            var bytesCount = registersCount*2;
            var bytes = new byte[bytesCount];
            for (var i = 0; i < bytesCount; i += 2)
            {
                var messageIndex = startIndex + i;
                bytes[i] = message[messageIndex];
                bytes[i + 1] = message[messageIndex + 1];
            }
            return bytes;
        }
    }
}
