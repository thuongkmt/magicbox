using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Machines;

namespace KonbiCloud.Diagnostic
{
    public class MachineError : CreationAuditedEntity, IMayHaveTenant
    {
        public virtual Machine Machine { get; set; }
        public string MachineErrorCode { get; set; }

        public string Message { get; set; }
        public string Solution { get; set; }
        public int? TenantId { get; set; }
    }
}