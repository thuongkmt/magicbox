using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System.Collections.Generic;

namespace KonbiCloud.Products
{
	[Table("ProductCategories")]
    public class ProductCategory : FullAuditedEntity<Guid> , IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public virtual string Name { get; set; }

        public virtual string Code { get; set; }

        public virtual string Desc { get; set; }

        public virtual ICollection<ProductCategoryRelation> ProductCategoryRelations { get; set; }
    }
}