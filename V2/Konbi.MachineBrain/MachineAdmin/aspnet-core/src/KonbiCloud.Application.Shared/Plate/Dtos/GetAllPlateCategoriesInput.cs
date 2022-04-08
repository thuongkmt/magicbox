using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.Plate.Dtos
{
    public class GetAllPlateCategoriesInput : PagedAndSortedResultRequestDto
    {
		public string Filter { get; set; }

		public string NameFilter { get; set; }

		public string DescFilter { get; set; }



    }
}