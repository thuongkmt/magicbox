using Abp.Application.Services.Dto;

namespace KonbiCloud.Products.Dtos
{
    public class GetAllForLookupTableInput : PagedAndSortedResultRequestDto
    {
		public string Filter { get; set; }
    }
}