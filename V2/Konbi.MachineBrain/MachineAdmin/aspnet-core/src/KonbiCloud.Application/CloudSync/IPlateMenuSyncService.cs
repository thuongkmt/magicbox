using KonbiCloud.PlateMenus.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public interface IPlateMenuSyncService
    {
        Task<List<MenuSchedule.PlateMenu>> Sync(Guid machineId);
        Task<bool> UpdateSyncStatus(SyncedItemData<Guid> input);
    }
}
