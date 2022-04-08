using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Enums;
using KonbiCloud.Restock;
using System;

namespace KonbiCloud.Inventories
{
    public class Topup: FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public Topup()
        {
            Id = Guid.NewGuid();
        }
        public int? TenantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int Total { get; set; }
        public int LeftOver { get; set; }
        public int Sold { get; set; }
        public int Error { get; set; }
        public bool IsProcessing { get; set; }

        public string RestockerName { get; set; }

        public TopupTypeEnum Type { get; set; }

        public virtual RestockSession RestockSession { get; set; }
    }
}
