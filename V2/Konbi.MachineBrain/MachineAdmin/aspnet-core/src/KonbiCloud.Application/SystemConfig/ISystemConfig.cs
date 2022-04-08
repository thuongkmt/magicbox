using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.SystemConfig.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.SystemConfig
{
    public interface ISystemConfig : IApplicationService
    {
        Task<ListResultDto<SystemConfigDto>> GetAll(SystemConfigListInput input);
        Task Update(UpdateConfigInput input);
    }
}
