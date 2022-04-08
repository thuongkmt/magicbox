using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public interface IPlateSyncService
    {
        Task<List<Plate.Plate>> Sync(Guid machineId);
        Task<bool> UpdateSyncStatus(SyncedItemData<Guid> input);
    }
}
