using System.Threading.Tasks;
using KonbiCloud.Common;

namespace KonbiCloud.CloudSync
{
    public interface ICloudSyncService<in T> where T:ISyncEntity
    {
        Task<bool> Sync(T entity);
    }
}