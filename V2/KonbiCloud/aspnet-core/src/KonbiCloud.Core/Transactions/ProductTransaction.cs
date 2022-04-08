using Abp.Domain.Entities.Auditing;
using KonbiCloud.Products;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonbiCloud.Transactions
{
    [Table("ProductTransactions")]
    public class ProductTransaction : CreationAuditedEntity<long>
    {
        public virtual Product Product { get; set; }

        public virtual DetailTransaction Transaction { get; set; }

        public decimal Amount { get; set; }
    }
}