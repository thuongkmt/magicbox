using Abp.Domain.Entities.Auditing;
using KonbiCloud.Inventories;
using KonbiCloud.Products;

namespace KonbiCloud.Transactions
{
    public class InventoryTransaction : CreationAuditedEntity
    {
        public virtual InventoryItem Inventory { get; set; }
        public virtual DetailTransaction Transaction { get; set; }
    }
}