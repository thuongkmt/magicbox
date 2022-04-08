using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Dto;
using System.Collections.Generic;

namespace KonbiCloud.Plate
{
    public interface IDiscsAppService : IApplicationService 
    {
        Task<PagedResultDto<GetDiscForView>> GetAll(GetAllDiscsInput input);

		Task<GetDiscForEditOutput> GetDiscForEdit(DiscDto input);

		Task CreateOrEdit(List<CreateOrEditDiscDto> input);

		Task Delete(DiscDto input);

		Task<FileDto> GetDiscsToExcel(GetAllDiscsForExcelInput input);

		
		Task<PagedResultDto<PlateLookupTableDto>> GetAllPlateForLookupTable(GetAllForLookupTableInput input);

        Task TestSendSignalRMessage(List<CreateOrEditDiscDto> input);

        Task UpdateSyncStatus(IEnumerable<DiscDto> dishes);
    }
}