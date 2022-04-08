using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.Plate.Dtos
{
    public class GetAllDiscsInput : PagedAndSortedResultRequestDto
    {
		public string Filter { get; set; }

		public string UidFilter { get; set; }

		public string CodeFilter { get; set; }


		public string PlateNameFilter { get; set; }
        public string PlateIdFilter { get; set; }
        

    }
}