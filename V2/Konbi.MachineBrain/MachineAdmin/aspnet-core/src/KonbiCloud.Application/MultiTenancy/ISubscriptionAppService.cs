using System.Threading.Tasks;
using Abp.Application.Services;

namespace KonbiCloud.MultiTenancy
{
    public interface ISubscriptionAppService : IApplicationService
    {
        Task UpgradeTenantToEquivalentEdition(int upgradeEditionId);
    }
}
