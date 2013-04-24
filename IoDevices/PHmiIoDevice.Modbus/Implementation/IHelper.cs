using System.Collections.Generic;

namespace PHmiIoDevice.Modbus.Implementation
{
    internal interface IHelper
    {
        void Open();

        List<byte> Read(int messageLength);

        void Write(byte[] message);

        void Close();
    }
}
