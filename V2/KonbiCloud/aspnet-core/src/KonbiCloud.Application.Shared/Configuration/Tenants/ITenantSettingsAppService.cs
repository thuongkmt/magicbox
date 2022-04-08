using System.Threading.Tasks;
using Abp.Application.Services;
using KonbiCloud.Configuration.Tenants.Dto;

namespace KonbiCloud.Configuration.Tenants
{
    public interface ITenantSettingsAppService : IApplicationService
    {
        Task<TenantSettingsEditDto> GetAllSettings();

        Task UpdateAllSettings(TenantSettingsEditDto input);

        Task ClearLogo();

        Task ClearCustomCss();
    }
}
