using System.Collections.Generic;
using KonbiCloud.Inventories.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Inventories.Exporting
{
    public interface IInventoriesExcelExporter
    {
        FileDto ExportToFile(List<GetInventoryForViewDto> inventories);
    }
}