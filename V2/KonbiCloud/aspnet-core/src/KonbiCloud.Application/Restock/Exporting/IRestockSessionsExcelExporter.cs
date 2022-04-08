using System.Collections.Generic;
using KonbiCloud.Restock.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Restock.Exporting
{
    public interface IRestockSessionsExcelExporter
    {
        FileDto ExportToFile(List<GetRestockSessionForViewDto> restockSessions);
    }
}