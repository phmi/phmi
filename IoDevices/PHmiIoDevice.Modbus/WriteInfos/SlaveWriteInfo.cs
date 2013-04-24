using System.Collections.Generic;

namespace PHmiIoDevice.Modbus.WriteInfos
{
    internal class SlaveWriteInfo
    {
        public readonly List<CoilWriteInfo> Coils = new List<CoilWriteInfo>();
        public readonly List<HoldingRegisterWriteInfo> HoldingRegisters
            = new List<HoldingRegisterWriteInfo>();
    }
}
