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

        Task<ListResultDto<InventoryOverviewDto>> GetInventoryOverview();

        Task<PagedResultDto<CurrentInventoryDto>> CurrentInventories(GetCurrentInventoryInput input);

        Task<InventoryDetailOutput> GetInventoryDetail(Guid machineId, Guid topupId);

        Task<GetInventoryForViewDto> GetInventoryForView(Guid id);

		Task<GetInventoryForEditOutput> GetInventoryForEdit(EntityDto<Guid> input);

		Task CreateOrEdit(CreateOrEditInventoryDto input);

		Task Delete(EntityDto<Guid> input);

		Task<FileDto> GetInventoriesToExcel(GetAllInventoriesForExcelInput input);

		Task<PagedResultDto<ProductLookupTableDto>> GetAllProductForLookupTable(GetAllForLookupTableInput input);

        Task<ListResultDto<InventoryOverviewDto>> GetMachinesInventoryRealTime();

        Task<List<InventoryDetailForRepportOutput>> GetInventoryForReport();
        bool RequestMachineToUpdateInventory(Guid machineId);
        Task<bool> Restock(RestockInput input);
    }
}