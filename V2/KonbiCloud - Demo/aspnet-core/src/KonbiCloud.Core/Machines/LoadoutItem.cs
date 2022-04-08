using System;
using System.Collections.Generic;
using System.Text;
using Abp.Domain.Entities;

namespace KonbiCloud.Machines
{
    public class LoadoutItem:Entity<Guid>, IMustHaveTenant
    {
        public string Code { get; set; }

        public bool IsEnable { get; set; }

        public string DocumentDbProductId { get; set; }
        public long LocalProductId { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public string Status { get; set; }

        public double Price { get; set; }
        public int TenantId { get; set; }
    }
}
