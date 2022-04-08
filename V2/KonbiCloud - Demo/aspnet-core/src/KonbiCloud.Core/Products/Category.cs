using System;
using System.Collections.Generic;
using System.Text;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Newtonsoft.Json;

namespace KonbiCloud.Products
{
    public class Category : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        public Category()
        {
            Products = new HashSet<ProductCategory>();
        }
        public int Status { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        [JsonIgnore]
        public virtual ICollection<ProductCategory> Products { get; set; }
        public int TenantId { get; set; }
    }
}
