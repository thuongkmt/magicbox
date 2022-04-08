using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Credit.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Credit
{
    public interface ICreditHistoriesAppService : IApplicationService 
    {
        Task<PagedResultDto<GetCreditHistoryForViewDto>> GetAll(GetAllCreditHistoriesInput input);

        Task<GetCreditHistoryForViewDto> GetCreditHistoryForView(Guid id);

		Task<GetCreditHistoryForEditOutput> GetCreditHistoryForEdit(EntityDto<Guid> input);

		Task CreateOrEdit(CreateOrEditCreditHistoryDto input);

		Task Delete(EntityDto<Guid> input);

		Task<FileDto> GetCreditHistoriesToExcel(GetAllCreditHistoriesForExcelInput input);

		
		Task<PagedResultDto<UserCreditLookupTableDto>> GetAllUserCreditForLookupTable(GetAllForLookupTableInput input);
		
    }
}