
using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Products.Dtos
{
    public class CreateOrEditProductCategoryDto : EntityDto<Guid?>
    {

		[Required]
		public string Name { get; set; }
		
		
		public string Code { get; set; }
		
		public string Desc { get; set; }

    }
}