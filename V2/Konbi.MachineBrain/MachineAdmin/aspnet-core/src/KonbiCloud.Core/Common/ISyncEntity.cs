using System;
using System.Collections.Generic;

namespace KonbiCloud.Common
{
    public interface ISyncEntity
    {
        bool IsSynced { get; set; }
        DateTime? SyncDate { get; set; }
    }

    public class SyncApiResponse<T>
    {
        public List<T> result { get; set; }
        public object targetUrl { get; set; }
        public bool success { get; set; }
        public object error { get; set; }
        public bool unAuthorizedRequest { get; set; }
        public bool __abp { get; set; }
    }
}
