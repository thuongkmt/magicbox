
using System;
using Abp.Application.Services.Dto;

namespace KonbiCloud.Credit.Dtos
{
    public class CreditHistoryDto : EntityDto<Guid>
    {
		public decimal Value { get; set; }

		public string Message { get; set; }

		public string Hash { get; set; }


		 public Guid? UserCreditId { get; set; }

		 
    }
}