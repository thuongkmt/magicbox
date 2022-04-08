using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Restock.Dtos
{
    public class GetRestockSessionForEditOutput
    {
		public CreateOrEditRestockSessionDto RestockSession { get; set; }


    }
}