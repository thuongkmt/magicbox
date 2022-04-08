using Abp.Application.Services;
using KonbiCloud.Settings.Dtos;
using System.Threading.Tasks;

namespace KonbiCloud.Settings
{
    public interface IAlertSettingAppServices : IApplicationService
    {
        AlertSettingDto GetAlertConfiguration(string machineID = "");
        Task<AlertConfiguration> CreateOrEdit(AlertSettingDto input);
    }
}
