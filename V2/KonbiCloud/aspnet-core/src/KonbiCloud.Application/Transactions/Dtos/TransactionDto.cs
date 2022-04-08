using KonbiCloud.Enums;
using KonbiCloud.Inventories;
using System;
using System.Collections.Generic;

namespace KonbiCloud.Transactions.Dtos
{
    public class TransactionDto
    {
        public TransactionDto()
        {
            Products = new List<ProductTransactionDto>();
        }

        public long Id { get; set; }
        public string TranCode { get; set; }
        public string Buyer { get; set; }
        public DateTime PaymentTime { get; set; }
        public decimal? Amount { get; set; }
        public string States { get; set; }
        public ICollection<ProductTransactionDto> Products { get; set; }
        public ICollection<TransactionDetailDto> TransactionDetails { get; set; }
        public string Machine { get; set; }
        public string Session { get; set; }
        public string TransactionId { get; set; }
        public decimal? PaidAmount { get; set; }
        public string CardLabel { get; set; }
        public string CardNumber { get; set; }
    }
}

