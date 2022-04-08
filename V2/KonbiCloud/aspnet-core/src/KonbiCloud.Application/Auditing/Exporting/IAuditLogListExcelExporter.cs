using System.Collections.Generic;
using KonbiCloud.Auditing.Dto;
using KonbiCloud.Dto;

namespace KonbiCloud.Auditing.Exporting
{
    public interface IAuditLogListExcelExporter
    {
        FileDto ExportToFile(List<AuditLogListDto> auditLogListDtos);

        FileDto ExportToFile(List<EntityChangeListDto> entityChangeListDtos);
    }
}
