using Abp.Application.Services.Dto;

namespace KonbiCloud.Plate.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
		public string Filter { get; set; }
    }
}