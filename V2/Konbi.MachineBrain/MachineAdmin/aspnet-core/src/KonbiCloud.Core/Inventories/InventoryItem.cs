using KonbiCloud.Products;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using KonbiCloud.Common;
using KonbiCloud.Transactions;
using Newtonsoft.Json;
using KonbiCloud.Enums;

namespace KonbiCloud.Inventories
{
    [Table("Inventories")]
    public class InventoryItem : FullAuditedEntity<Guid>, IMayHaveTenant, ISyncEntity
    {
        public InventoryItem()
        {
            Id=Guid.NewGuid();
        }

        public TagState State { get; set; }

        public int? TenantId { get; set; }
        [Required]
        public virtual string TagId { get; set; }

        public virtual int? TrayLevel { get; set; }

        public virtual double Price { get; set; }

        public virtual Guid ProductId { get; set; }
        public Product Product { get; set; }

        public Guid? TopupId { get; set; }
        [ForeignKey("TopupId")]
        public virtual Topup Topup { get; set; }

        public bool IsSynced { get; set; }
        public DateTime? SyncDate { get; set; }

        public long? DetailTransactionId { get; set; }
        [JsonIgnore]

        public virtual DetailTransaction Transaction { get; set; }
    }
}