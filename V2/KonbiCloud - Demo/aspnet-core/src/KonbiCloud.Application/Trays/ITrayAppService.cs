using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Tray.Dtos;

namespace KonbiCloud.Plate
{
    public interface ITrayAppService : IApplicationService 
    {
        Task<PagedResultDto<TrayDto>> GetAllTrays(TrayRequest request);

        Task<TrayDto> CreateOrEditTray(TrayDto input);

        Task Delete(EntityDto<Guid> input);
    }
}