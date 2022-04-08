
using System;
using Abp.Application.Services.Dto;

namespace KonbiCloud.Plate.Dtos
{
    public class DiscDto : EntityDto<Guid>
    {
		public string Uid { get; set; }

		public string Code { get; set; }

		public Guid PlateId { get; set; }
    }
}