using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using KonbiCloud.DataExporting.Excel.EpPlus;
using KonbiCloud.Products.Dtos;
using KonbiCloud.Dto;
using KonbiCloud.Storage;

namespace KonbiCloud.Products.Exporting
{
    public class ProductsExcelExporter : EpPlusExcelExporterBase, IProductsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public ProductsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
			ITempFileCacheManager tempFileCacheManager) :  
	base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetProductForViewDto> products)
        {
            return CreateExcelPackage(
               "Products.xlsx",
               excelPackage =>
               {
                   var sheet = excelPackage.Workbook.Worksheets.Add(L("Products"));
                   sheet.OutLineApplyStyle = true;

                   AddHeader
                   (
                       sheet,
                       L("Name"),
                       L("SKU"),
                       L("Barcode"),
                       L("ShortDesc"),
                       L("Desc"),
                       L("Tag"),
                       L("ImageUrl"),
                       L("Price")
                   );

                   AddObjects
                   (
                       sheet, 2, products,
                       _ => _.Product.Name,
                       _ => _.Product.SKU,
                       _ => _.Product.Barcode,
                       _ => _.Product.ShortDesc,
                       _ => _.Product.Desc,
                       _ => _.Product.Tag,
                       _ => _.Product.ImageUrl,
                       _ => _.Product.Price
                   );
               });
        }
    }
}
