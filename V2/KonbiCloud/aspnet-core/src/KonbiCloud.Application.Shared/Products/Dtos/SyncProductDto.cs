using System;
using System.Collections.Generic;

namespace KonbiCloud.Products.Dtos
{
    public class SyncProductDto
    {
        public SyncProductDto()
        {
            Categories = new List<SyncProductCategoryDto>();
        }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string SKU { get; set; }

        public string Barcode { get; set; }

        public double? Price { get; set; }

        public string ShortDesc { get; set; }

        public string Desc { get; set; }

        public string Tag { get; set; }

        public string ImageUrl { get; set; }

        public List<SyncProductCategoryDto> Categories { get; set; }

        public bool IsDeleted { get; set; }
    }

    public class SyncProductCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Desc { get; set; }
    }
}
