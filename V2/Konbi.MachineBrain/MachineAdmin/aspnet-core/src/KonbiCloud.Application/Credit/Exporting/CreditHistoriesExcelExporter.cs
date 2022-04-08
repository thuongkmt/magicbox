using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using KonbiCloud.DataExporting.Excel.EpPlus;
using KonbiCloud.Credit.Dtos;
using KonbiCloud.Dto;
using KonbiCloud.Storage;

namespace KonbiCloud.Credit.Exporting
{
    public class CreditHistoriesExcelExporter : EpPlusExcelExporterBase, ICreditHistoriesExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public CreditHistoriesExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
			ITempFileCacheManager tempFileCacheManager) :  
	base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetCreditHistoryForViewDto> creditHistories)
        {
            return CreateExcelPackage(
                "CreditHistories.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.Workbook.Worksheets.Add(L("CreditHistories"));
                    sheet.OutLineApplyStyle = true;

                    AddHeader(
                        sheet,
                        L("Value"),
                        L("Message"),
                        L("Hash"),
                        (L("UserCredit")) + L("UserId")
                        );

                    AddObjects(
                        sheet, 2, creditHistories,
                        _ => _.CreditHistory.Value,
                        _ => _.CreditHistory.Message,
                        _ => _.CreditHistory.Hash,
                        _ => _.UserCreditUserId
                        );

					

                });
        }
    }
}
