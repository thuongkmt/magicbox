
using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace KonbiCloud.Products.Dtos
{
    public class ProductDto : EntityDto<Guid>
    {
        public string Name { get; set; }

        public string SKU { get; set; }

		public string Barcode { get; set; }

        public string ShortDesc { get; set; }

        public string Desc { get; set; }

		public string Tag { get; set; }

		public string ImageUrl { get; set; }
        public string TagPrefix { get; set; }

        public double? Price { get; set; }

        public DateTime CreationTime { get; set; }

        public List<string> CategoriesName { get; set; }
    }
}