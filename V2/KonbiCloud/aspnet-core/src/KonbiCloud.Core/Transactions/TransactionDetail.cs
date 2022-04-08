using KonbiCloud.Products;
using KonbiCloud.Transactions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;

namespace KonbiCloud.Transactions
{
    [Table("TransactionDetails")]
    public class TransactionDetail : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        public virtual string TagId { get; set; }

        public virtual double Price { get; set; }

        public virtual string TopupId { get; set; }

        public virtual string MachineId { get; set; }

        public virtual string LocalInventoryId { get; set; }

        public virtual Guid ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public virtual long? TransactionId { get; set; }

        [ForeignKey("TransactionId")]
        public DetailTransaction DetailTransaction { get; set; }

    }
}