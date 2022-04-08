using System.Collections.Generic;
using KonbiCloud.Products.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Products.Exporting
{
    public interface IProductCategoriesExcelExporter
    {
        FileDto ExportToFile(List<GetProductCategoryForViewDto> productCategories);
    }
}