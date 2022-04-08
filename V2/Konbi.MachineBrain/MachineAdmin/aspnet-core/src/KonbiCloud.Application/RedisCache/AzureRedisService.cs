using System;
using System.Collections.Generic;
using System.Linq;
using KonbiCloud.Configuration;
using KonbiCloud.Machines;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace KonbiCloud.RedisCache
{
    public class AzureRedisService : IAzureRedisService, IDisposable
    {
        private readonly Lazy<ConnectionMultiplexer> _lazyConnection;
        private readonly IDatabase _cache;

        public AzureRedisService(IOptions<RedisOption> redisOption)
        {
            _lazyConnection =
                new Lazy<ConnectionMultiplexer>(() =>
                    ConnectionMultiplexer.Connect(redisOption.Value.AzureConnectionString));
            _cache = _lazyConnection.Value.GetDatabase();
        }

        public void Dispose()
        {
            _lazyConnection.Value.Dispose();
        }

        public T Get<T>(string key) where T : class
        {
            var stringCache = _cache.StringGet(key);
            return !stringCache.IsNullOrEmpty ? JsonConvert.DeserializeObject<T>(stringCache) : null;
        }

        public void Set<T>(T value, string key)
        {
            _cache.StringSet(key, JsonConvert.SerializeObject(value));
        }

        private MachineListCache GetMachineList()
        {
            return this.Get<MachineListCache>(MachineListCache.GetCacheName());
        }

        public void UpdateMachine(MachineCacheItem item)
        {
            var machines = this.Get<MachineListCache>(MachineListCache.GetCacheName());
            var machine = machines.Items.SingleOrDefault(x => x.MachineId == item.MachineId);
            if (machine == null) machines.Items.Add(item);
            else
            {
                machine = item;
                machine.IsOffline = item.IsOffline;
                machine.LogicalId = item.LogicalId;
            }

            this.Set(machines, MachineListCache.GetCacheName());
        }

       
        public void UpdateMachineList(List<MachineCacheItem> machines)
        {
            var newList = new MachineListCache() {Items = machines};
            this.Set(newList,MachineListCache.GetCacheName());
        }
    }
}
