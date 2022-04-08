using Abp.Application.Services.Dto;

namespace KonbiCloud.Machines.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
		public string Filter { get; set; }
    }
}