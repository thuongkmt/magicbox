using System;
using System.Collections.Generic;
using System.Text;
using static KonbiCloud.BackgroundJobs.StopSaleMessageService;

namespace KonbiCloud.Machines
{
    public interface IStopSaleAppService
    {
        void ChangeMachineStatus(MachineStatus status);
    }
}
