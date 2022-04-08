using System;
using System.Collections.Generic;
using System.Text;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace KonbiCloud.Prices
{
    public class PriceStrategyCode : FullAuditedEntity<int>, IMustHaveTenant
    {
        public int TenantId { get; set; }
        public string Code { get; set; }
    }

    public class PriceStrategy : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        public int TenantId { get; set; }
        public decimal? Value { get; set; }

        public int? PriceCodeId { get; set; }
        public Guid? PlateMenuId { get; set; }
        public virtual PriceStrategyCode PriceCode { get; set; }
    }

}
