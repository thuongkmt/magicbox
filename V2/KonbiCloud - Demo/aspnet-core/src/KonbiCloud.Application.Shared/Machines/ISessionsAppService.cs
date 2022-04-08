using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Machines
{
    public interface ISessionsAppService : IApplicationService 
    {
        Task<PagedResultDto<GetSessionForView>> GetAll(GetAllSessionsInput input);

		Task<GetSessionForEditOutput> GetSessionForEdit(EntityDto<Guid> input);

		Task CreateOrEdit(CreateOrEditSessionDto input);

		Task Delete(EntityDto<Guid> input);

		Task<FileDto> GetSessionsToExcel(GetAllSessionsForExcelInput input);

		
    }
}