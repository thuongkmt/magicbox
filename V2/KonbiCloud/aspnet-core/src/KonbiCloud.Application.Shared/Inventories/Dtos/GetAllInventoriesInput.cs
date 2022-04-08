using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.Inventories.Dtos
{
    public class GetAllInventoriesInput : PagedAndSortedResultRequestDto
    {
		public string Filter { get; set; }

		public string TagIdFilter { get; set; }

		public int? MaxTrayLevelFilter { get; set; }
		public int? MinTrayLevelFilter { get; set; }

		public double? MaxPriceFilter { get; set; }
		public double? MinPriceFilter { get; set; }


		 public string ProductNameFilter { get; set; }

		 
    }
}