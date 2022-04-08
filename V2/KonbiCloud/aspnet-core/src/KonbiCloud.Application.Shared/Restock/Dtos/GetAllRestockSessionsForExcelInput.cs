using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.Restock.Dtos
{
    public class GetAllRestockSessionsForExcelInput
    {
		public string Filter { get; set; }

		public DateTime? MaxStartDateFilter { get; set; }
		public DateTime? MinStartDateFilter { get; set; }

		public DateTime? MaxEndDateFilter { get; set; }
		public DateTime? MinEndDateFilter { get; set; }

		public int? MaxTotalFilter { get; set; }
		public int? MinTotalFilter { get; set; }

		public int? MaxLeftOverFilter { get; set; }
		public int? MinLeftOverFilter { get; set; }

		public int? MaxSoldFilter { get; set; }
		public int? MinSoldFilter { get; set; }

		public int? MaxErrorFilter { get; set; }
		public int? MinErrorFilter { get; set; }

		public int IsProcessingFilter { get; set; }

		public string RestockerNameFilter { get; set; }

		public int? MaxRestockedFilter { get; set; }
		public int? MinRestockedFilter { get; set; }

		public int? MaxUnloadedFilter { get; set; }
		public int? MinUnloadedFilter { get; set; }



    }
}