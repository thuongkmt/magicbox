using Abp.Application.Services.Dto;
using System;

namespace KonbiCloud.Transactions.Dtos
{
    public class TransactionInput : PagedAndSortedResultRequestDto
    {
		public string SessionFilter { get; set; }

        public string StateFilter { get; set; }

        //type 0: error, type 1: success
        public int TransactionType { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? MachineFilter { get; set; }
        public string CardLabel { get; set; }

    }
}