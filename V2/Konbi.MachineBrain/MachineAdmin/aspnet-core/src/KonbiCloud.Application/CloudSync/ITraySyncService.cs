using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public interface ITraySyncService
    {
        Task<List<Plate.Tray>> Sync(Guid machineId);
    }
}
