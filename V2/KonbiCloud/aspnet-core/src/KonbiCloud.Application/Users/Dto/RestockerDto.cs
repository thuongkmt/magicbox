
using Abp.Application.Services.Dto;

namespace KonbiCloud.Users
{
    public class RestockerTokenDto
    {
        public string UserName { get; set; }
        public string Token { get; set; }
    }

    public class RestockerDto : EntityDto<long>
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string EmailAddress { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string PasswordRepeat { get; set; }

        public string PassCode { get; set; }

        public string QrCode { get; set; }
    }

    public class GetAllRestockerInput : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }
    }
}
