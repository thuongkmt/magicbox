using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Caching.Dto;

namespace KonbiCloud.Caching
{
    public interface ICachingAppService : IApplicationService
    {
        ListResultDto<CacheDto> GetAllCaches();

        Task ClearCache(EntityDto<string> input);

        Task ClearAllCaches();
    }
}
