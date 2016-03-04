using System;
using PHmiClient.Utils;
using PHmiClient.Utils.Notifications;
using PHmiModel;
using PHmiModel.Entities;

namespace PHmiRunner.Utils.IoDeviceRunner
{
    public class IoDeviceRunTargetFactory : IIoDeviceRunTargetFactory
    {
        public IIoDeviceRunTarget Create(ITimeService timeService, IoDevice ioDevice)
        {
            return new IoDeviceRunTarget(
                ioDevice,
                new IoDeviceWrapperFactory(string.Format("{0} domain", ioDevice.Name)),
                new NotificationReporter(timeService){LifeTime = TimeSpan.FromTicks(0)});
        }
    }
}
