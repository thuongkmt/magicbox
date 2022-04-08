using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Common
{
    public interface ISyncEntity
    {
        bool IsSynced { get; set; }
        DateTime? SyncDate { get; set; }
    }
}
