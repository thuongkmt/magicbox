using System.Collections.Generic;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Machines.Exporting
{
    public interface ISessionsExcelExporter
    {
        FileDto ExportToFile(List<GetSessionForView> sessions);
    }
}