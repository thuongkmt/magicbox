using KonbiCloud.Plate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public interface IPlateCategorySyncService
    {
        Task<List<PlateCategory>> Sync(Guid mId);
    }
}
