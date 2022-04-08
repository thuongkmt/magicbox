using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.TemperatureLogs.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.TemperatureLogs
{
    public interface ITemperatureLogsAppService : IApplicationService
    {
        Task<ListResultDto<TemperatureLogListDto>> GetTemperatureLogs(GetTemperatureLogInput input);
    }
}
