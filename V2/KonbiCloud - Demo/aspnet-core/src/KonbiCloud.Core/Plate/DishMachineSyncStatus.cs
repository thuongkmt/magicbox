using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonbiCloud.Plate
{
    [Table("DishMachineSyncStatus")]
    public class DishMachineSyncStatus : FullAuditedEntity<Guid> , IMayHaveTenant, ISyncEntity
    {
		public int? TenantId { get; set; }
        public Guid DiscId { get; set; }
        public Guid MachineId { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncDate { get; set; }

    }
}