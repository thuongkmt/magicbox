using System.Threading.Tasks;
using Abp.Application.Services;
using KonbiCloud.Install.Dto;

namespace KonbiCloud.Install
{
    public interface IInstallAppService : IApplicationService
    {
        Task Setup(InstallDto input);

        AppSettingsJsonDto GetAppSettingsJson();

        CheckDatabaseOutput CheckDatabase();
    }
}