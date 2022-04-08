using System;
using System.Collections.Generic;

namespace KonbiCloud.RedisCache
{
    public class VendingMachineStatusCache
    {
        private const string CACHE_KEY_SUFFIX = "AzureRedisCache";
        public static string GetListCacheName()
        {
            //var idStr = id == null ? "" : id.ToString();
            return "VendingMachineStatusCacheList" + CACHE_KEY_SUFFIX;
        }
        public string VmcState { get; set; }
        public string VmcType { get; set; }
        public string MachineType { get; set; }
        public string Name { get; set; }
        public string MachineId { get; set; }
        public float Temperature { get; set; }
        public string DispenseErrorCount { get; set; }
        public bool IsOffline { get; set; }
        public DateTime? LastModified { get; set; }
        public IEnumerable<VendingDeviceStatusDto> DeviceStatus { get; set; }
    }
}