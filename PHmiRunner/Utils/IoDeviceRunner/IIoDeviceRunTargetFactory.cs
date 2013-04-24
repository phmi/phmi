using PHmiClient.Utils;
using PHmiModel;

namespace PHmiRunner.Utils.IoDeviceRunner
{
    public interface IIoDeviceRunTargetFactory
    {
        IIoDeviceRunTarget Create(ITimeService timeService, io_devices ioDevice);
    }
}
