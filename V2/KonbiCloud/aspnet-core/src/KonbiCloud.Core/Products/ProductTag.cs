using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonbiCloud.Products
{
    [Table("ProductTags")]
    public class ProductTag : FullAuditedEntity<Guid>, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public string Name { get; set; }

        [ForeignKey("Product")]
        public Guid ProductId { get; set; }

        public Product Product { get; set; }

        public ProductTagStateEnum State { get; set; }
    }
}
