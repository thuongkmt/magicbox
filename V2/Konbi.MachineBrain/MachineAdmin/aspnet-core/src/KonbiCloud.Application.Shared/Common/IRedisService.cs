using Abp.Dependency;

namespace KonbiCloud.Common
{
    public interface IRedisService:ITransientDependency
    {
        void PublishCommandToMachine(string machineId, string command);
    }
}
