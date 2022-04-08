
using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace KonbiCloud.Plate.Dtos
{
    public class PlateDto : EntityDto<Guid>
    {
		public string Name { get; set; }

		public string ImageUrl { get; set; }

		public string Desc { get; set; }

		public string Code { get; set; }

		public int? Avaiable { get; set; }

		public string Color { get; set; }

        public int? PlateCategoryId { get; set; }

        public ICollection<DiscDto> Discs { get; set; }
    }
}