
using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;

namespace KonbiCloud.Plate.Dtos
{
    public class PlateCategoryDto : EntityDto
    {
		public string Name { get; set; }

		public string Desc { get; set; }

        public ICollection<PlateDto> Plates { get; set; }

    }
}