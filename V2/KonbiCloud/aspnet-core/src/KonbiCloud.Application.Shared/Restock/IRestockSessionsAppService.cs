using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Restock.Dtos;
using KonbiCloud.Dto;


namespace KonbiCloud.Restock
{
    public interface IRestockSessionsAppService : IApplicationService 
    {
        Task<PagedResultDto<GetRestockSessionForViewDto>> GetAll(GetAllRestockSessionsInput input);

        Task<GetRestockSessionForViewDto> GetRestockSessionForView(int id);

		Task<GetRestockSessionForEditOutput> GetRestockSessionForEdit(EntityDto input);

		Task CreateOrEdit(CreateOrEditRestockSessionDto input);

		Task Delete(EntityDto input);

		Task<FileDto> GetRestockSessionsToExcel(GetAllRestockSessionsForExcelInput input);

		
    }
}