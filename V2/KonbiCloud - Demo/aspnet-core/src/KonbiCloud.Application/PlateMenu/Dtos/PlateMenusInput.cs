using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.PlateMenus.Dtos
{
    public class PlateMenusInput : PagedAndSortedResultRequestDto
    {
		public string NameFilter { get; set; }

        public string CodeFilter { get; set; }

        public int? CategoryFilter { get; set; }

		public string ColorFilter { get; set; }

		public DateTime? DateFilter { get; set; }

		public string SessionFilter { get; set; }

        public string Id { get; set; }
        public decimal Price { get; set; }

        public string PriceStrategyId { get; set; }
        public decimal PriceStrategy { get; set; }
    }
}