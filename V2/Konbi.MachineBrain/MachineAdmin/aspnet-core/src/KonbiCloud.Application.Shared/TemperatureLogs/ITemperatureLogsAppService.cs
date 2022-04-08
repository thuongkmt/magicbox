using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using KonbiCloud.TemperatureLogs.Dtos;

namespace KonbiCloud.TemperatureLogs
{
    public interface ITemperatureLogsAppService
    {
        Task<ListResultDto<TemperatureLogListDto>> GetTemperatureLogs(GetTemperatureLogInput input);

        Task AddTemperatureLog(decimal temperatureLog);
    }
}
