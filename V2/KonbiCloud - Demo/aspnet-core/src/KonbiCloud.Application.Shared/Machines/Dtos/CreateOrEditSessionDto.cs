
using System;
using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace KonbiCloud.Machines.Dtos
{
    public class CreateOrEditSessionDto : EntityDto<Guid?>
    {

		[Required]
		public string Name { get; set; }
		
		
		public string FromHrs { get; set; }
		
		
		public string ToHrs { get; set; }
		
		

    }
}