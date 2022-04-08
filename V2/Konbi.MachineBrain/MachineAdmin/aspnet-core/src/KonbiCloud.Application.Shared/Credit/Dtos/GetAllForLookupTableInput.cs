using Abp.Application.Services.Dto;

namespace KonbiCloud.Credit.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
		public string Filter { get; set; }
    }
}