using System;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace KonbiCloud.Employees
{
    public class Employee : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        public string Name { get; set; }
        public string CardId { get; set; }
        public double Quota { get; set; }
        public string Period { get; set; }
        public double Ordered { get; set; }
        public int TenantId { get; set; }
        public bool QuotaCash { get; set; }
    }
}
