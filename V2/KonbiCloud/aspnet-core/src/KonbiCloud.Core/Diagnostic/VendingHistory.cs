using System;
using System.Collections.Generic;
using System.Text;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Machines;

namespace KonbiCloud.Diagnostic
{
    public class VendingHistory : CreationAuditedEntity<long>, IMayHaveTenant
    {
        public virtual Machine Machine { get; set; }
        public string VmcLevel { get; set; }
        public decimal Temperature { get; set; }
        public int? TenantId { get; set; }
    }
}
