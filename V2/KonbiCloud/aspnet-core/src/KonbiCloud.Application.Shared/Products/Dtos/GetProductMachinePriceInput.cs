using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.Products.Dtos
{
    public class GetProductMachinePriceInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public Guid CategoryFilter { get; set; }
    }
}
