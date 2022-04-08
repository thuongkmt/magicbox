using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using KonbiCloud.DataExporting.Excel.EpPlus;
using KonbiCloud.Credit.Dtos;
using KonbiCloud.Dto;
using KonbiCloud.Storage;

namespace KonbiCloud.Credit.Exporting
{
    public class UserCreditsExcelExporter : EpPlusExcelExporterBase, IUserCreditsExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public UserCreditsExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
			ITempFileCacheManager tempFileCacheManager) :  
	base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetUserCreditForViewDto> userCredits)
        {
            return CreateExcelPackage(
                "UserCredits.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.Workbook.Worksheets.Add(L("UserCredits"));
                    sheet.OutLineApplyStyle = true;

                    AddHeader(
                        sheet,
                        L("Value"),
                        L("Hash"),
                        (L("User")) + L("Name")
                        );

                    AddObjects(
                        sheet, 2, userCredits,
                        _ => _.UserCredit.Value,
                        _ => _.UserCredit.Hash,
                        _ => _.UserName
                        );

					

                });
        }
    }
}
