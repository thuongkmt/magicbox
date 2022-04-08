using KonbiCloud.Dto;

namespace KonbiCloud.Common.Dto
{
    public class FindUsersInput : PagedAndFilteredInputDto
    {
        public int? TenantId { get; set; }
    }
}