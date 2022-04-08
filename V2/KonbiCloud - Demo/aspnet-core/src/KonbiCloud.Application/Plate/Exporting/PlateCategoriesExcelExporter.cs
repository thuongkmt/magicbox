using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using KonbiCloud.DataExporting.Excel.EpPlus;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Dto;
using KonbiCloud.Storage;

namespace KonbiCloud.Plate.Exporting
{
    public class PlateCategoriesExcelExporter : EpPlusExcelExporterBase, IPlateCategoriesExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public PlateCategoriesExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
			ITempFileCacheManager tempFileCacheManager) :  
	base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetPlateCategoryForView> plateCategories)
        {
            return CreateExcelPackage(
                "PlateCategories.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.Workbook.Worksheets.Add(L("PlateCategories"));
                    sheet.OutLineApplyStyle = true;

                    AddHeader(
                        sheet,
                        L("Name"),
                        L("Desc")
                        );

                    AddObjects(
                        sheet, 2, plateCategories,
                        _ => _.PlateCategory.Name,
                        _ => _.PlateCategory.Desc
                        );

					

                });
        }
    }
}
