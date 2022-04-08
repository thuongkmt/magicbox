
using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Plate.Dtos
{
    public class CreateOrEditDiscDto : EntityDto<Guid?>
    {

		[Required]
		public string Uid { get; set; }
		
		
		[Required]
		public string Code { get; set; }
		
		
		public Guid PlateId { get; set; }
    }
}