using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Credit.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Credit
{
    public interface IUserCreditsAppService : IApplicationService 
    {
        Task<PagedResultDto<GetUserCreditForViewDto>> GetAll(GetAllUserCreditsInput input);

        Task<GetUserCreditForViewDto> GetUserCreditForView(Guid id);

		Task<GetUserCreditForEditOutput> GetUserCreditForEdit(EntityDto<Guid> input);

		Task CreateOrEdit(CreateOrEditUserCreditDto input);

		Task Delete(EntityDto<Guid> input);

		Task<FileDto> GetUserCreditsToExcel(GetAllUserCreditsForExcelInput input);

		
		Task<PagedResultDto<UserLookupTableDto>> GetAllUserForLookupTable(GetAllForLookupTableInput input);
		
    }
}