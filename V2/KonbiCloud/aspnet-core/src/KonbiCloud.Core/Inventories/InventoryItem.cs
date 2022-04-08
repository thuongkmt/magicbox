using KonbiCloud.Products;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using KonbiCloud.Enums;
using KonbiCloud.Machines;
using KonbiCloud.Transactions;

namespace KonbiCloud.Inventories
{
    [Table("Inventories")]
    public class InventoryItem : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required] public virtual string TagId { get; set; }

        public virtual int? TrayLevel { get; set; }

        public virtual double Price { get; set; }

        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }

        public Guid? TopupId { get; set; }
        [ForeignKey("TopupId")] public virtual Topup Topup { get; set; }

        public Guid MachineId { get; set; }
        [ForeignKey("MachineId")] public virtual Machine Machine { get; set; }

        public long? DetailTransactionId { get; set; }
        public virtual DetailTransaction Transaction { get; set; }
        public TagState State { get; set; }
    }
}
