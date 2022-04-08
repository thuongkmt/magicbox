using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using KonbiCloud.DataExporting.Excel.EpPlus;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.Dto;
using KonbiCloud.Storage;

namespace KonbiCloud.Machines.Exporting
{
    public class SessionsExcelExporter : EpPlusExcelExporterBase, ISessionsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public SessionsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
			ITempFileCacheManager tempFileCacheManager) :  
	base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetSessionForView> sessions)
        {
            return CreateExcelPackage(
                "Sessions.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.Workbook.Worksheets.Add(L("Sessions"));
                    sheet.OutLineApplyStyle = true;

                    AddHeader(
                        sheet,
                        L("Name"),
                        L("FromHrs"),
                        L("ToHrs")
                        );

                    AddObjects(
                        sheet, 2, sessions,
                        _ => _.Session.Name,
                        _ => _.Session.FromHrs,
                        _ => _.Session.ToHrs
                        );

					

                });
        }
    }
}
