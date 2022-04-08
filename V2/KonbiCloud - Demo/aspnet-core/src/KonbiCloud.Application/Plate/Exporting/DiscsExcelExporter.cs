using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using KonbiCloud.DataExporting.Excel.EpPlus;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Dto;
using KonbiCloud.Storage;

namespace KonbiCloud.Plate.Exporting
{
    public class DiscsExcelExporter : EpPlusExcelExporterBase, IDiscsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public DiscsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
			ITempFileCacheManager tempFileCacheManager) :  
	base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetDiscForView> discs)
        {
            return CreateExcelPackage(
                "Discs.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.Workbook.Worksheets.Add(L("Discs"));
                    sheet.OutLineApplyStyle = true;

                    AddHeader(
                        sheet,
                        L("Uid"),
                        L("Code"),
                        (L("Plate")) + L("Name")
                        );

                    AddObjects(
                        sheet, 2, discs,
                        _ => _.Disc.Uid,
                        _ => _.Disc.Code,
                        _ => _.PlateName
                        );

					

                });
        }
    }
}
