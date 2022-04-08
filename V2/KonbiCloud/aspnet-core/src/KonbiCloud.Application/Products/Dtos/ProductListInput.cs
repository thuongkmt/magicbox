using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.Products.Dtos
{
    public class ProductListInput : IPagedAndSortedResultRequest
    {
        public string CategoryId { get; set; }
        public int MaxResultCount { get; set; }
        public int SkipCount { get; set; }
        public string Sorting { get; set; }

        public Guid? MachineId { get; set; }
        public string ProductName { get; set; }
    }
}
