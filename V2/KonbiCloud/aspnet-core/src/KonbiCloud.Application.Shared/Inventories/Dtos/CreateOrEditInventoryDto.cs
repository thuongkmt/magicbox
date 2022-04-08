
using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Inventories.Dtos
{
    public class CreateOrEditInventoryDto : EntityDto<Guid?>
    {

		[Required]
		public string TagId { get; set; }
		
		
		public int? TrayLevel { get; set; }
		
		
		public double Price { get; set; }
		
		
		 public Guid ProductId { get; set; }
		 
		 
    }
}