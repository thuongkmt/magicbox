using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Products.Dtos
{
    public class UpdateProductMachinePriceInput : PagedAndSortedResultRequestDto
    {
        public Guid MachineId { get; set; }

        public Guid ProductId { get; set; }

        public decimal Price { get; set; }
    }
}
