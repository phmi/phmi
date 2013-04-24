using PHmiClient.Utils;

namespace PHmiClient.PHmiSystem
{
    internal class UpdateStatusRunTargetFactory : IUpdateStatusRunTargetFactory
    {
        public IUpdateStatusRunTarget Create(ITimeService timeService)
        {
            return new UpdateStatusRunTarget(timeService);
        }
    }
}
