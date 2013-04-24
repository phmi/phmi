using PHmiIoDeviceTools;

namespace PHmiRunner.Utils.IoDeviceRunner
{
    public interface IIoDeviceWrapper
    {
        void Create(string filePath, string options);
        object[] Read(ReadParameter[] readParameters);
        void Write(WriteParameter[] writeParameters);
        void Dispose();
    }
}
