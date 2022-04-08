using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using KonbiCloud.DataExporting.Excel.EpPlus;
using KonbiCloud.Restock.Dtos;
using KonbiCloud.Dto;
using KonbiCloud.Storage;

namespace KonbiCloud.Restock.Exporting
{
    public class RestockSessionsExcelExporter : EpPlusExcelExporterBase, IRestockSessionsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public RestockSessionsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
			ITempFileCacheManager tempFileCacheManager) :  
	base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetRestockSessionForViewDto> restockSessions)
        {
            return CreateExcelPackage(
                "RestockSessions.xlsx",
                excelPackage =>
                {
                    
                    var sheet = excelPackage.Workbook.Worksheets.Add(L("RestockSessions"));
                    sheet.OutLineApplyStyle = true;

                    AddHeader(
                        sheet,
                        L("StartDate"),
                        L("EndDate"),
                        L("Total"),
                        L("LeftOver"),
                        L("Sold"),
                        L("Error"),
                        L("IsProcessing"),
                        L("RestockerName"),
                        L("Restocked"),
                        L("Unloaded")
                        );

                    AddObjects(
                        sheet, 2, restockSessions,
                        _ => _timeZoneConverter.Convert(_.RestockSession.StartDate, _abpSession.TenantId, _abpSession.GetUserId()),
                        _ => _timeZoneConverter.Convert(_.RestockSession.EndDate, _abpSession.TenantId, _abpSession.GetUserId()),
                        _ => _.RestockSession.Total,
                        _ => _.RestockSession.LeftOver,
                        _ => _.RestockSession.Sold,
                        _ => _.RestockSession.Error,
                        _ => _.RestockSession.IsProcessing,
                        _ => _.RestockSession.RestockerName,
                        _ => _.RestockSession.Restocked,
                        _ => _.RestockSession.Unloaded
                        );

					var startDateColumn = sheet.Column(1);
                    startDateColumn.Style.Numberformat.Format = "yyyy-mm-dd";
					startDateColumn.AutoFit();
					var endDateColumn = sheet.Column(2);
                    endDateColumn.Style.Numberformat.Format = "yyyy-mm-dd";
					endDateColumn.AutoFit();
					
					
                });
        }
    }
}
