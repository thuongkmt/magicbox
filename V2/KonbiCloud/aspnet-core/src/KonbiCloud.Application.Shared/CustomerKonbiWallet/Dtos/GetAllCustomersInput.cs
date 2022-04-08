using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.CustomerKonbiWallet.Dtos
{
    public class GetAllCustomersInput : PagedAndSortedResultRequestDto
    {
        public string Filter { get; set; }
        public string CustomerFilter { get; set; }
        public string UserNameFilter { get; set; }
        public string EmailFilter { get; set; }
    }
}
