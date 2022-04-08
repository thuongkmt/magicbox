using Abp.Application.Services.Dto;

namespace KonbiCloud.Inventories.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
		public string Filter { get; set; }
    }
}