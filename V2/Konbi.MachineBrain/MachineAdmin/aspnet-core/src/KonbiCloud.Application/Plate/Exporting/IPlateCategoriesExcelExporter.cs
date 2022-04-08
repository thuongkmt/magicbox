using System.Collections.Generic;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Plate.Exporting
{
    public interface IPlateCategoriesExcelExporter
    {
        FileDto ExportToFile(List<GetPlateCategoryForView> plateCategories);
    }
}