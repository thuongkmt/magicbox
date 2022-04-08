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

        public FileDto ExportToFile(List<ProductDto> products)
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
                        _ => _.Name,
                        _ => _.SKU,
                        _ => _.Barcode,
                        _ => _.ShortDesc,
                        _ => _.Desc,
                        _ => _.Tag,
                        _ => _.ImageUrl,
                        _ => _.Price
                    );
                });
        }
    }
}
