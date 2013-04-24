using System;
using PHmiClient.Loc;
using PHmiClient.Utils;
using PHmiClient.Wcf;

namespace PHmiClient.PHmiSystem
{
    internal class UpdateStatusRunTarget : IUpdateStatusRunTarget
    {
        private readonly ITimeService _timeService;

        public UpdateStatusRunTarget(ITimeService timeService)
        {
            _timeService = timeService;
        }

        public void Run(IService service)
        {
            var updateStatusResult = service.UpdateStatus();
            _timeService.UtcTime = updateStatusResult.Time;
        }

        public void Clean()
        {
        }

        public string Name { get { return Res.StatusService; } }
    }
}
