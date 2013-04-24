using PHmiRunner.Utils.Alarms;
using PHmiRunner.Utils.IoDeviceRunner;
using System.Collections.Generic;
using PHmiRunner.Utils.Logs;
using PHmiRunner.Utils.Trends;
using PHmiRunner.Utils.Users;

namespace PHmiRunner.Utils
{
    public interface IProject
    {
        IDictionary<int, IIoDeviceRunTarget> IoDeviceRunTargets { get; }
        IUsersRunner UsersRunner { get; }
        IDictionary<int, IAlarmsRunTarget> AlarmsRunTargets { get; }
        IDictionary<int, ITrendsRunTarget> TrendsRunTargets { get; }
        IDictionary<int, ILogMaintainer> LogMaintainers { get; } 
    }
}
