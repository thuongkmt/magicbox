using System;
using System.Collections.Generic;
using System.Text;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using KonbiCloud.Products;

namespace KonbiCloud.Categories.Dtos
{

    [AutoMapFrom(typeof(Category))]
    public class CategoryListDto : FullAuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public int TenantId { get; set; }
        public string ImageUrl { get; set; }
        public string FileContent { get; set; }
    }
    
}
