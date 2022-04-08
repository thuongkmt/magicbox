using System;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Inventories.Dtos;
using KonbiCloud.Dto;
using System.Collections.Generic;

namespace KonbiCloud.Inventories
{
    public interface IInventoriesAppService : IApplicationService 
    {
        Task<PagedResultDto<GetInventoryForViewDto>> GetAll(GetAllInventoriesInput input);

        Task<GetCurrentTopupDto> GetCurrentTopupInfo();

        Task<GetInventoryForViewDto> GetInventoryForView(Guid id);

		Task<GetInventoryForEditOutput> GetInventoryForEdit(EntityDto<Guid> input);

		Task CreateOrEdit(CreateOrEditInventoryDto input);

		Task Delete(EntityDto<Guid> input);

		Task<FileDto> GetInventoriesToExcel(GetAllInventoriesForExcelInput input);

		Task<PagedResultDto<ProductLookupTableDto>> GetAllProductForLookupTable(GetAllForLookupTableInput input);

        Task Topup(List<CreateOrEditInventoryDto> items);

        Task<TopupDto> GetCurrentTopup();

        Task UnmapAll();

        Task UnloadItems(UnloadInputDto input);
        Task RestockWithSession(RestockWithSessionInputDto input);

         Task UnloadItemsByTagId(UnloadByTagIdInputDto input);
    }
}