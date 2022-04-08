using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KonbiCloud.Products
{
	[Table("Products")]
    public class Product : FullAuditedEntity<Guid> , IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public virtual string Name { get; set; }

        [Required]
        public virtual string SKU { get; set; }

        public virtual string Barcode { get; set; }

        public virtual double? Price { get; set; }

        public virtual string ShortDesc { get; set; }

        public virtual string Desc { get; set; }

        public virtual string Tag { get; set; }

        public virtual string ImageUrl { get; set; }

        [JsonIgnore]

        public virtual ICollection<ProductCategoryRelation> ProductCategoryRelations { get; set; }
    }
}