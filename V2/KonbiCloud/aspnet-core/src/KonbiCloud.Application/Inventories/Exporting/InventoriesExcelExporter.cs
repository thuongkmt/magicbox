using System.Collections.Generic;
using Abp.Runtime.Session;
using Abp.Timing.Timezone;
using KonbiCloud.DataExporting.Excel.EpPlus;
using KonbiCloud.Inventories.Dtos;
using KonbiCloud.Dto;
using KonbiCloud.Storage;

namespace KonbiCloud.Inventories.Exporting
{
    public class InventoriesExcelExporter : EpPlusExcelExporterBase, IInventoriesExcelExporter
    {

        private readonly ITimeZoneConverter _timeZoneConverter;
        private readonly IAbpSession _abpSession;

        public InventoriesExcelExporter(
            ITimeZoneConverter timeZoneConverter,
            IAbpSession abpSession,
			ITempFileCacheManager tempFileCacheManager) :  
	base(tempFileCacheManager)
        {
            _timeZoneConverter = timeZoneConverter;
            _abpSession = abpSession;
        }

        public FileDto ExportToFile(List<GetInventoryForViewDto> inventories)
        {
            return CreateExcelPackage(
                "Inventories.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.Workbook.Worksheets.Add(L("Inventories"));
                    sheet.OutLineApplyStyle = true;

                    AddHeader(
                        sheet,
                        L("TagId"),
                        L("TrayLevel"),
                        L("Price"),
                        (L("Product")) + L("Name")
                        );

                    AddObjects(
                        sheet, 2, inventories,
                        _ => _.Inventory.TagId,
                        _ => _.Inventory.TrayLevel,
                        _ => _.Inventory.Price,
                        _ => _.ProductName
                        );

					

                });
        }
    }
}
