using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;

namespace KonbiCloud.Products.Dtos
{
    public class ProductMachinePriceListDto : FullAuditedEntity
    {
        public Guid MachineId { get; set; }

        public string MachineName { get; set; }

        public Guid ProductId { get; set; }

        public string ProductName { get; set; }

        public virtual string SKU { get; set; }

        public decimal Price { get; set; }
    }
}
