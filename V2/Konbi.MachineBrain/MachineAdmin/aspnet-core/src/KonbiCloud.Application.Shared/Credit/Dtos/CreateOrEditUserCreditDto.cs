
using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Credit.Dtos
{
    public class CreateOrEditUserCreditDto : EntityDto<Guid?>
    {

		public decimal Value { get; set; }
		
		
		public string Hash { get; set; }
		
		
		 public long? UserId { get; set; }
		 
		 
    }
}