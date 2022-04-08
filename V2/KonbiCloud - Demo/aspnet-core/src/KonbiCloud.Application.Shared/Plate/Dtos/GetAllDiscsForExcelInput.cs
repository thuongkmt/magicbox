using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.Plate.Dtos
{
    public class GetAllDiscsForExcelInput
    {
		public string Filter { get; set; }

		public string UidFilter { get; set; }

		public string CodeFilter { get; set; }


		 public string PlateNameFilter { get; set; }

		 
    }
}