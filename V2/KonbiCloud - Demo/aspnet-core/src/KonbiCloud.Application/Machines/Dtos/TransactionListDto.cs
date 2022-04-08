using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace KonbiCloud.Machines.Dtos
{
    //[AutoMapFrom(typeof(DetailTransaction))]
    public class TransactionListDto : FullAuditedEntityDto<Guid>
    {
        public string MachineID { get; set; }
        public string ItemLocation { get; set; }
        public DateTime SaleTime { get; set; }
        public decimal TotalValue { get; set; }
        public decimal SumCollected { get; set; }
        public decimal ChangeIssued { get; set; }
        public string PaymentMethod { get; set; }
        public string ProductName { get; set; }

    }
}