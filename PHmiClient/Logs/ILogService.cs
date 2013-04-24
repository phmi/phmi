using PHmiClient.PHmiSystem;

namespace PHmiClient.Logs
{
    internal interface ILogService : IServiceRunTarget
    {
        void Add(LogAbstract log);
    }
}
