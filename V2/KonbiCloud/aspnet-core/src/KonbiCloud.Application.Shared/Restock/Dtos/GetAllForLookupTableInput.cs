using Abp.Application.Services.Dto;

namespace KonbiCloud.Restock.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
		public string Filter { get; set; }
    }
}