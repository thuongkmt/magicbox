using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonbiCloud.Inventories
{
    [Table("TopupInventory")]
    public class TopupHistory : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public Guid TopupId { get; set; }
        [ForeignKey("TopupId")]
        public virtual Topup Topup { get; set; }
        public Guid? InventoryId { get; set; }

        [ForeignKey("InventoryId")]
        public virtual InventoryItem InventoryItem { get; set; }
        public string Tag { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
        public TopupHistoryType Type { get; set; }

       
    }
}
