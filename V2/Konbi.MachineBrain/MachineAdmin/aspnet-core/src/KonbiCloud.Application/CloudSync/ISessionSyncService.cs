using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public interface ISessionSyncService
    {
        Task<List<Sessions.Session>> Sync(Guid machineId);
    }
}
