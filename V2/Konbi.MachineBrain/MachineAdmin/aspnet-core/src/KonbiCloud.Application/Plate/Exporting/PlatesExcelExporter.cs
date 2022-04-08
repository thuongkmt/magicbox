using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using KonbiCloud.DataExporting.Excel.EpPlus;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Dto;
using KonbiCloud.Storage;

namespace KonbiCloud.Plate.Exporting
{
    public class PlatesExcelExporter : EpPlusExcelExporterBase, IPlatesExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public PlatesExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
			ITempFileCacheManager tempFileCacheManager) :  
	base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetPlateForView> plates)
        {
            return CreateExcelPackage(
                "Plates.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.Workbook.Worksheets.Add(L("Plates"));
                    sheet.OutLineApplyStyle = true;

                    AddHeader(
                        sheet,
                        L("Name"),
                        L("ImageUrl"),
                        L("Desc"),
                        L("Code"),
                        L("Avaiable"),
                        L("Color"),
                        (L("PlateCategory")) + L("Name")
                        );

                    AddObjects(
                        sheet, 2, plates,
                        _ => _.Plate.Name,
                        _ => _.Plate.ImageUrl,
                        _ => _.Plate.Desc,
                        _ => _.Plate.Code,
                        _ => _.Plate.Avaiable,
                        _ => _.Plate.Color,
                        _ => _.PlateCategoryName
                        );

					

                });
        }
    }
}
