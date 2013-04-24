using System;
using PHmiClient.Utils.Runner;

namespace PHmiRunner.Utils.IoDeviceRunner
{
    public interface IIoDeviceRunTarget : IRunTarget, IDisposable
    {
        bool? GetDigitalValue(int id);
        void SetDigitalValue(int id, bool value);
        double? GetNumericValue(int id);
        void SetNumericValue(int id, double value);
        void EnterReadLock();
        void ExitReadLock();
        void EnterWriteLock();
        void ExitWriteLock();
    }
}
