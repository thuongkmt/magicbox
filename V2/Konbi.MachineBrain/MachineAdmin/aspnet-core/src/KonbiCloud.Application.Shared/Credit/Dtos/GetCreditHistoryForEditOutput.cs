using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Credit.Dtos
{
    public class GetCreditHistoryForEditOutput
    {
		public CreateOrEditCreditHistoryDto CreditHistory { get; set; }

		public string UserCreditUserId { get; set;}


    }
}