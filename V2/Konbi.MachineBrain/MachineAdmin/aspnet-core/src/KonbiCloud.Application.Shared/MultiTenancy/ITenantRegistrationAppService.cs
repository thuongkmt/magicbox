using System.Threading.Tasks;
using Abp.Application.Services;
using KonbiCloud.Editions.Dto;
using KonbiCloud.MultiTenancy.Dto;

namespace KonbiCloud.MultiTenancy
{
    public interface ITenantRegistrationAppService: IApplicationService
    {
        Task<RegisterTenantOutput> RegisterTenant(RegisterTenantInput input);

        Task<EditionsSelectOutput> GetEditionsForSelect();

        Task<EditionSelectDto> GetEdition(int editionId);
    }
}