using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using KonbiCloud.DataExporting.Excel.EpPlus;
using KonbiCloud.Products.Dtos;
using KonbiCloud.Dto;
using KonbiCloud.Storage;

namespace KonbiCloud.Products.Exporting
{
    public class ProductCategoriesExcelExporter : EpPlusExcelExporterBase, IProductCategoriesExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public ProductCategoriesExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
			ITempFileCacheManager tempFileCacheManager) :  
	base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetProductCategoryForViewDto> productCategories)
        {
            return CreateExcelPackage(
                "ProductCategories.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.Workbook.Worksheets.Add(L("ProductCategories"));
                    sheet.OutLineApplyStyle = true;

                    AddHeader
                    (
                        sheet,
                        L("Name"),
                        L("Code"),
                        L("Desc")
                    );

                    AddObjects
                    (
                        sheet, 2, productCategories,
                        _ => _.ProductCategory.Name,
                        _ => _.ProductCategory.Code,
                        _ => _.ProductCategory.Desc
                    );
                });
        }
    }
}
