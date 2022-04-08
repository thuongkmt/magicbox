using System;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KonbiCloud.Machines;
using KonbiCloud.Products;
using Abp.Timing;

namespace KonbiCloud.Products
{
    [Table("ProductMachinePrice")]
    public class ProductMachinePrice : FullAuditedEntity, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid MachineId { get; set; }

        public Machine Machine { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        public Product Product { get; set; }

        [Required]
        public decimal Price { get; set; }

        public ProductMachinePrice()
        {
            CreationTime = Clock.Now;
        }
    }
}
