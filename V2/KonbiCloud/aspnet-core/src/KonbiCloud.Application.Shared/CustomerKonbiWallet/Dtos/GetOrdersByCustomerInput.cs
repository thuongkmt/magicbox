using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.CustomerKonbiWallet.Dtos
{
    public class GetOrdersByCustomerInput : PagedAndSortedResultRequestDto
    {
        public long CustomerId { get; set; }
    }
}
