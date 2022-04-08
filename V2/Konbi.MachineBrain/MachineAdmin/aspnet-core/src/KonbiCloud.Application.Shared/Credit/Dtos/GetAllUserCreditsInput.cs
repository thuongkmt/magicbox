using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.Credit.Dtos
{
    public class GetAllUserCreditsInput : PagedAndSortedResultRequestDto
    {
		public string Filter { get; set; }

		public decimal? MaxValueFilter { get; set; }
		public decimal? MinValueFilter { get; set; }

		public string HashFilter { get; set; }


		 public string UserNameFilter { get; set; }

		 
    }
}