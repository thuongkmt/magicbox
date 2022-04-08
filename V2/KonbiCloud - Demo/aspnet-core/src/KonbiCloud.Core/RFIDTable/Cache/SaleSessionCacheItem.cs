using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.RFIDTable.Cache
{
    public class SaleSessionCacheItem
    {
        public static readonly string CacheName = "SaleSessionCacheItem";
        
        public SessionInfo SessionInfo { get; set; }
        public List<MenuItemInfo> MenuItems { get;  set; }

    }
}
