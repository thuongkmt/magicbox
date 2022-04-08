using Abp.Application.Services.Dto;
using KonbiCloud.Enums;
using System;
using System.Collections.Generic;

namespace KonbiCloud.Products.Dtos
{
    public class ProductTagDto : EntityDto<Guid>
    {
        public string Name { get; set; }
        public string ProductName { get; set; }
        public DateTime CreationTime { get; set; }
        public ProductTagStateEnum State {get;set;}
    }

    public class GetProductTagForViewDto
    {
        public ProductTagDto ProductTag { get; set; }
    }

    public class ProductTagForReportDto : EntityDto<Guid>
    {
        public string ProductName { get; set; }
        public string Category { get; set; }
        public string Sku { get; set; }
        public string ExpiryDate { get; set; }
        public string TagId { get; set; }
        public string State { get; set; }

        public DateTime CreationTime { get; set; }
        public DateTime PurchaseDate { get; set; }

    }


    public class TagsInput
    {
        public Guid MachineId { get; set; }
        public IList<string> Tags { get; set; }
    }

    public class ProductInfo
    {
        public string Tag { get; set; }
        public Guid? ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal? Price { get; set; }
    }

    public class ListProductTagDto
    {
        public ListProductTagDto()
        {
            ListTags = new List<ProductTagInputDto>();
        }

        public List<ProductTagInputDto> ListTags { get; set; }

        public int? TenantId { get; set; }
    }

    public class ProductTagInputDto
    {
        public string Name { get; set; }
        public string ProductId { get; set; }
    }
}
