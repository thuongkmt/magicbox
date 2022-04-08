using KonbiCloud.Plate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public interface IDishSyncService : ICloudSyncService<Disc>
    {
        Task<List<Disc>> SyncFromServer(Guid machineId);
        Task<bool> UpdateSyncStatus(SyncedItemData<Guid> input);
        Task<bool> PushToServer(SyncedItemData<Disc> dishes);
    }
}
