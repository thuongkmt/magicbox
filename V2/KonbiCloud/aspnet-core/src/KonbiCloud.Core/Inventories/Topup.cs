
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Enums;
using KonbiCloud.Restock;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KonbiCloud.Inventories
{
    public class Topup : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public Guid? PreviousTopupId { get; set; }
        [ForeignKey("PreviousTopupId")]
        public virtual Topup PreviousTopup { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Total { get; set; }
        public int LeftOver { get; set; }

        public int Sold { get; set; }
        public int Error { get; set; }
        public bool IsInprogress { get; set; }
        public string RestockerName { get; set; }

        public TopupTypeEnum Type { get; set; }
        public Guid MachineId { get; set; }
        [ForeignKey("MachineId")]
        public virtual Machines.Machine Machine { get; set; }

        public int? RestockSessionId { get; set; }
        [ForeignKey("RestockSessionId")]
        public virtual RestockSession RestockSession { get; set; }
        public virtual ICollection<TopupHistory> History { get; set; }
        public virtual ICollection<InventoryItem> Inventory { get; set; }
    }
}
