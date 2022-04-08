using System;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace KonbiCloud.Diagnostic
{
    public class HardwareDiagnosticDetail: FullAuditedEntity<long>, IMustHaveTenant
    {
        public int TenantId { get; set; }
        public string Detail { get; set; }
        public DateTime DateTime { get; set; }
        public long HardwareDiagnosticId { get; set; }
        public long OriginId { get; set; }
        public int Level { get; set; }
    }
}
