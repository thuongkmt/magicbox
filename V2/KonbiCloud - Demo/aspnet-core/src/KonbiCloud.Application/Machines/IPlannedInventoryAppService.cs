using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Dependency;

namespace KonbiCloud.Machines
{
    public interface IPlannedInventoryAppService:ITransientDependency
    {
        Task InitPlannedInventory(Machine machine);
        Task GetPlannedInventory(Guid machineId, Session session);
    }
}
