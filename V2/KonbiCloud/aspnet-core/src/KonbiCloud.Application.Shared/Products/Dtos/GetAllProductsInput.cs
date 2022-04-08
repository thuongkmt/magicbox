using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.Products.Dtos
{
    public class GetAllProductsInput : PagedAndSortedResultRequestDto
    {
		public string Filter { get; set; }

        public string NameFilter { get; set; }

        public string SKUFilter { get; set; }

        public string BarcodeFilter { get; set; }

        public string ShortDescFilter { get; set; }

        public string DescFilter { get; set; }

        public string TagFilter { get; set; }

        public Guid CategoryFilter { get; set; }

        public double? MaxPriceFilter { get; set; }

		public double? MinPriceFilter { get; set; }

		public string ImageUrlFilter { get; set; }
    }
}