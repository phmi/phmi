using PHmiClient.Utils;

namespace PHmiClient.PHmiSystem
{
    internal interface IUpdateStatusRunTargetFactory
    {
        IUpdateStatusRunTarget Create(ITimeService timeService);
    }
}
