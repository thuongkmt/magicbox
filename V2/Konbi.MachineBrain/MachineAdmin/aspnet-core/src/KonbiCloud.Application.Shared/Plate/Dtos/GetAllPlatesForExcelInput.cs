using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.Plate.Dtos
{
    public class GetAllPlatesForExcelInput
    {
		public string Filter { get; set; }

		public string NameFilter { get; set; }

		public string ImageUrlFilter { get; set; }

		public string DescFilter { get; set; }

		public string CodeFilter { get; set; }

		public int? MaxAvaiableFilter { get; set; }
		public int? MinAvaiableFilter { get; set; }

		public string ColorFilter { get; set; }


		 public string PlateCategoryNameFilter { get; set; }

		 
    }
}