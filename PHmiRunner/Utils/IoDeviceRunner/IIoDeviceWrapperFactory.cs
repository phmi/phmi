namespace PHmiRunner.Utils.IoDeviceRunner
{
    public interface IIoDeviceWrapperFactory
    {
        IIoDeviceWrapper Create();
        void UnloadDomain();
    }
}
