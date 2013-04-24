using System;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiModel;

namespace PHmiRunner.Utils.IoDeviceRunner
{
    public class IoDeviceRunTargetFactory : IIoDeviceRunTargetFactory
    {
        public IIoDeviceRunTarget Create(ITimeService timeService, io_devices ioDevice)
        {
            return new IoDeviceRunTarget(
                ioDevice,
                new IoDeviceWrapperFactory(string.Format("{0} domain", ioDevice.name)),
                new NotificationReporter(timeService){LifeTime = TimeSpan.FromTicks(0)});
        }
    }
}
