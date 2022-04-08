using Abp.Domain.Entities.Auditing;
using KonbiCloud.Plate;
using System;

namespace KonbiCloud.Transactions
{
    public class DishTransaction : CreationAuditedEntity
    {
        public Guid? DiscId { get; set; }
        public virtual Disc Disc { get; set; }
        public virtual DetailTransaction Transaction { get; set; }

        public decimal Amount { get; set; }

    }
}