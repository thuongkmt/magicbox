using System.Collections.Generic;
using Abp.Dependency;
using KonbiCloud.Machines;

namespace KonbiCloud.RedisCache
{
    public interface IAzureRedisService : ITransientDependency
    {
        void Set<T>(T value, string key);
        T Get<T>(string key) where T : class;
        void UpdateMachine(MachineCacheItem item);
        void UpdateMachineList(List<Machine> machines);
        void UpdateMachineList(List<MachineCacheItem> machines);
    }
}
