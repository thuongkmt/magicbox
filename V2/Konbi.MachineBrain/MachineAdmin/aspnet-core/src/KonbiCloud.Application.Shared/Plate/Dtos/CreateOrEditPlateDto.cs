
using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Plate.Dtos
{
    public class CreateOrEditPlateDto : EntityDto<Guid?>
    {
		public string Name { get; set; }
		
		public string ImageUrl { get; set; }

        public string Desc { get; set; }

        [Required]
		public string Code { get; set; }
		
		public int? Avaiable { get; set; }
		
		public string Color { get; set; }
		
		public int? PlateCategoryId { get; set; }
    }
}