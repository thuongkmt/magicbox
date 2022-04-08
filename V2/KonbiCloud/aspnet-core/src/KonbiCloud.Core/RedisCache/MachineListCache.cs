using System;
using System.Collections.Generic;
using System.Reflection;

namespace KonbiCloud.RedisCache
{
    public class MachineListCache:CacheBase
    {
        private const string CACHE_KEY_SUFFIX = "AzureRedisCache";
        public static string GetCacheName(Guid? id = null)
        {
            var idStr = id == null ? "" : id.ToString();
            return MethodBase.GetCurrentMethod().DeclaringType.Name + idStr + CACHE_KEY_SUFFIX;
        }

        private List<MachineCacheItem> items;

        public List<MachineCacheItem> Items
        {
            get => items ?? (items = new List<MachineCacheItem>());
            set => items = value;
        }
    }

    public class MachineCacheItem
    {
        public Guid MachineId { get; set; }
        public string LogicalId { get; set; }
        public bool IsOffline { get; set; }
    }
}
