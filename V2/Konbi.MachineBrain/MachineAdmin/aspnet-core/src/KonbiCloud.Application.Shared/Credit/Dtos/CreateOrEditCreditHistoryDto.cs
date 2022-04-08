
using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Credit.Dtos
{
    public class CreateOrEditCreditHistoryDto : EntityDto<Guid?>
    {

		public decimal Value { get; set; }
		
		
		[Required]
		public string Message { get; set; }
		
		
		public string Hash { get; set; }
		
		
		 public Guid? UserCreditId { get; set; }
		 
		 
    }
}