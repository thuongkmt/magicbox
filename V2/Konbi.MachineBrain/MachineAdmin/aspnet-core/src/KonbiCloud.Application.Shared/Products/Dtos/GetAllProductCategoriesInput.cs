using Abp.Application.Services.Dto;

namespace KonbiCloud.Products.Dtos
{
    public class GetAllProductCategoriesInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }

        public string NameFilter { get; set; }

        public string CodeFilter { get; set; }

        public string DescFilter { get; set; }
    }
}