
using System;
using Abp.Application.Services.Dto;

namespace KonbiCloud.Machines.Dtos
{
    public class SessionDto : EntityDto<Guid>
    {
		public string Name { get; set; }

		public string FromHrs { get; set; }

		public string ToHrs { get; set; }



    }
}