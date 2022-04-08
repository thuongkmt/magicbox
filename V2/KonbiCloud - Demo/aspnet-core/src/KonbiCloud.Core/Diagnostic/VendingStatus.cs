using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Machines;

namespace KonbiCloud.Diagnostic
{
   
    public class VendingStatus : CreationAuditedEntity, IMayHaveTenant
    {
        public string MachineID { get; set; }
        public string VmcLevel { get; set; }
        public bool VmcOk { get; set; }
        public bool IucOk { get; set; }
        public bool CykloneOk { get; set; }
        public bool MdbOk { get; set; }
        public bool IsSynced { get; set; }
        public float Temperature { get; set; }

        public string SnapshotUrl { get; set; }
        public int? TenantId { get; set; }
    }
}