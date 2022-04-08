using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Plate
{
    public interface IPlatesAppService : IApplicationService 
    {
        Task<PagedResultDto<GetPlateForView>> GetAll(GetAllPlatesInput input);

		Task<GetPlateForEditOutput> GetPlateForEdit(EntityDto<Guid> input);

		Task<PlateMessage> CreateOrEdit(CreateOrEditPlateDto input);

		Task Delete(EntityDto<Guid> input);

		Task<FileDto> GetPlatesToExcel(GetAllPlatesForExcelInput input);
		
		Task<PagedResultDto<PlateCategoryLookupTableDto>> GetAllPlateCategoryForLookupTable(GetAllForLookupTableInput input);
    }
}