using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Application.Services.Dto;

namespace KonbiCloud.Products.Dtos
{
    public class ProductDto : FullAuditedEntityDto<Guid>
    {
        public ProductDto()
        {
            Categories=new List<Guid>();
        }
        public string SKU { get; set; }
        public string Name { get; set; }

        public int Status { get; set; }
        public string Unit { get; set; }
        public float Price { get; set; }
        public string ImageUrl { get; set; }
        public  IList<Guid> Categories { get; set; }

        public int TenantId { get; set; }
        public string Barcode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ShortDesc1 { get; set; }
        public string ShortDesc2 { get; set; }
        public string ShortDesc3 { get; set; }
        public string Desc { get; set; }
        public string CategoryIds => string.Join(",", Categories);
    }
}