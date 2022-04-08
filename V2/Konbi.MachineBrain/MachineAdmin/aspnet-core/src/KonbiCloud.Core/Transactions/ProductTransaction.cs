using Abp.Domain.Entities.Auditing;
using KonbiCloud.Products;

namespace KonbiCloud.Transactions
{
    public class ProductTransaction : CreationAuditedEntity
    {
        public virtual Product Product { get; set; }
        public virtual DetailTransaction Transaction { get; set; }
        public decimal Amount { get; set; }
    }
}