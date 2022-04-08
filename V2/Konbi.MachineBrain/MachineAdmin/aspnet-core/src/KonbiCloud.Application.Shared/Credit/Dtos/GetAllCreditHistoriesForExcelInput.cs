using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.Credit.Dtos
{
    public class GetAllCreditHistoriesForExcelInput
    {
		public string Filter { get; set; }

		public decimal? MaxValueFilter { get; set; }
		public decimal? MinValueFilter { get; set; }

		public string MessageFilter { get; set; }

		public string HashFilter { get; set; }


		 public string UserCreditUserIdFilter { get; set; }

		 
    }
}