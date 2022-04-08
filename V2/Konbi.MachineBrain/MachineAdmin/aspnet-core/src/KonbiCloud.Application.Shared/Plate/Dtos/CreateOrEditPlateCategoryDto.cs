
using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Plate.Dtos
{
    public class CreateOrEditPlateCategoryDto : EntityDto<int?>
    {

		[Required]
		public string Name { get; set; }
		
		
		public string Desc { get; set; }
		
		

    }
}