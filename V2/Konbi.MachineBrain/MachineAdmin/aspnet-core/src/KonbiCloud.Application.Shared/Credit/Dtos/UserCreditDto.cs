
using System;
using Abp.Application.Services.Dto;

namespace KonbiCloud.Credit.Dtos
{
    public class UserCreditDto : EntityDto<Guid>
    {
		public decimal Value { get; set; }

		public string Hash { get; set; }


		 public long? UserId { get; set; }

		 
    }
}