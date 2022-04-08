using System.Collections.Generic;
using KonbiCloud.Products.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Products.Exporting
{
    public interface IProductsExcelExporter
    {
        FileDto ExportToFile(List<GetProductForViewDto> products);
    }
}