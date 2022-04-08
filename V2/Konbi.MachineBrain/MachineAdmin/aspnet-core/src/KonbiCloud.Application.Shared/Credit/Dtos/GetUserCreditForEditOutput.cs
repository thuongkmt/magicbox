using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Credit.Dtos
{
    public class GetUserCreditForEditOutput
    {
		public CreateOrEditUserCreditDto UserCredit { get; set; }

		public string UserName { get; set;}


    }
}