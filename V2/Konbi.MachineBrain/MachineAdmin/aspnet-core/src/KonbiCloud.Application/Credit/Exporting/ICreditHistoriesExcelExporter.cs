using System.Collections.Generic;
using KonbiCloud.Credit.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Credit.Exporting
{
    public interface ICreditHistoriesExcelExporter
    {
        FileDto ExportToFile(List<GetCreditHistoryForViewDto> creditHistories);
    }
}