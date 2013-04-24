using PHmiClient.PHmiSystem;

namespace PHmiClient.Tags
{
    internal interface ITagService : IServiceRunTarget
    {
        void Add(IoDeviceAbstract ioDevice);
    }
}
