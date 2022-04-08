using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.Products.Dtos
{
    public class GetAllProductTagsInput : PagedAndSortedResultRequestDto
    {
        public string TagFilter { get; set; }
        
        public string ProductFilter { get; set; }

        public int? StateFilter { get; set; }

        public DateTime? FromDateFilter { get; set; }
        
        public DateTime? ToDateFilter { get; set; }
    }
}
