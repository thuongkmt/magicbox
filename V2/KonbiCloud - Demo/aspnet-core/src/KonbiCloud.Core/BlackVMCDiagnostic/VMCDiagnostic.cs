using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;

namespace KonbiCloud.BlackVMCDiagnostic
{
    public class VMCDiagnostic : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public string MachineId { get; set; }
        public DateTime LogTime { get; set; }
        public bool LevelA { get; set; }
        public bool LevelB { get; set; }
        public bool LevelC { get; set; }
        public bool LevelD { get; set; }
        public bool LevelE { get; set; }
        public bool LevelF { get; set; }
        public int? TenantId { get; set; }
    }
}
