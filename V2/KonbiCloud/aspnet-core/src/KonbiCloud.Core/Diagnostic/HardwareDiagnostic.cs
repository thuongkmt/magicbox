using System;
using System.Collections.Generic;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace KonbiCloud.Diagnostic
{
    public class HardwareDiagnostic: FullAuditedEntity<long>, IMustHaveTenant
    {
        public long OriginId { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Element { get; set; }
        public DateTime? OriginCreatedDate { get; set; }
        public string MachineName { get; set; }
        public string MachineId { get; set; }
    }
}
