using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonbiCloud.MenuSchedule
{
    [Table("PlateMenuMachineSyncStatus")]
    public class PlateMenuMachineSyncStatus : FullAuditedEntity<Guid> , IMayHaveTenant, ISyncEntity
    {
		public int? TenantId { get; set; }
        public Guid PlateMenuId { get; set; }
        public Guid MachineId { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncDate { get; set; }

    }
}