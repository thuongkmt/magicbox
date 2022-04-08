using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.Machines.Dtos
{
    public class GetAllSessionsInput : PagedAndSortedResultRequestDto
    {
		public string Filter { get; set; }

		public string NameFilter { get; set; }

		public string FromHrsFilter { get; set; }

		public string ToHrsFilter { get; set; }



    }
}