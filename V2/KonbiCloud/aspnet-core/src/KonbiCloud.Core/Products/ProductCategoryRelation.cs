using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonbiCloud.Products
{
    [Table("ProductCategoryRelations")]
    public class ProductCategoryRelation : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public virtual int? TenantId { get; set; }

        [Required]
        [ForeignKey("Product")]
        public virtual Guid ProductId { get; set; }

        [Required]
        [ForeignKey("ProductCategory")]
        public virtual Guid ProductCategoryId { get; set; }

        public Product Product { get; set; }

        public ProductCategory ProductCategory { get; set; }
    }
}
