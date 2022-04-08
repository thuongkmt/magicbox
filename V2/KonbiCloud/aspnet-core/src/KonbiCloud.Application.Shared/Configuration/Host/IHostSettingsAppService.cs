using System.Threading.Tasks;
using Abp.Application.Services;
using KonbiCloud.Configuration.Host.Dto;

namespace KonbiCloud.Configuration.Host
{
    public interface IHostSettingsAppService : IApplicationService
    {
        Task<HostSettingsEditDto> GetAllSettings();

        Task UpdateAllSettings(HostSettingsEditDto input);

        Task SendTestEmail(SendTestEmailInput input);
    }
}
