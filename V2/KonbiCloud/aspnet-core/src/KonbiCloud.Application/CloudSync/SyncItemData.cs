using System;
using System.Collections.Generic;

namespace KonbiCloud.CloudSync
{
    public class SyncedItemData<T>
    {
        public Guid MachineId { get; set; }
        public IList<T> SyncedItems { get; set; }
    }
}
