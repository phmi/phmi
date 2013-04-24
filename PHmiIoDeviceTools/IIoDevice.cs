using System;

namespace PHmiIoDeviceTools
{
    public interface IIoDevice : IDisposable
    {
        void Open();
        object[] Read(ReadParameter[] readParameters);
        void Write(WriteParameter[] writeParameters);
    }
}
