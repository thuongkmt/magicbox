using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Inventories;
using KonbiCloud.Machines;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonbiCloud.Restock
{
    public class RestockSessionHistory : FullAuditedEntity<Guid>
    {
        public Guid RestockSessionId { get; set; }
        [ForeignKey("RestockSessionId")]
        public Topup RestockSession { get; set; }
        public Guid LoadoutItemId { get; set; }
        [ForeignKey("LoadoutItemId")]
        public LoadoutItem LoadoutItem { get; set; }
        public Guid? OldProduct { get; set; }
        public Guid? NewProduct { get; set; }
        public string PriceChange { get; set; }
        public string QuantityChange { get; set; }
        public string CapacityChange { get; set; }
    }
}
