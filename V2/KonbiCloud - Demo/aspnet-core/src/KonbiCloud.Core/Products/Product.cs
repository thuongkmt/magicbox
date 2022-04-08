using System;
using System.Collections.Generic;
using System.Text;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace KonbiCloud.Products
{
    public class Product : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        public Product()
        {
            Categories = new HashSet<ProductCategory>();
        }
        public string SKU { get; set; }
        public string Name { get; set; }
        
        public int Status { get; set; }
        public string Unit { get; set; }
        public float Price { get; set; }
        public string ImageUrl { get; set; }
        public string ImageChecksum { get; set; }

        public virtual ICollection<ProductCategory> Categories { get; set; }

        public int TenantId { get; set; }
        public string Barcode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ShortDesc1 { get; set; }
        public string ShortDesc2 { get; set; }
        public string ShortDesc3 { get; set; }
        public string Desc { get; set; }
    }
}
