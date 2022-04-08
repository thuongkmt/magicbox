using KonbiCloud.Enums;
using System;
using System.Collections.Generic;

namespace KonbiCloud.Transactions.Dtos
{
    public class TransactionDto
    {
        public long Id { get; set; }
        public string TranCode { get; set; }
        public string Buyer { get; set; }
        public DateTime PaymentTime { get; set; }
        public decimal? Amount { get; set; }
        public int PlatesQuantity { get; set; }
        public string States { get; set; }
        public ICollection<DishTransactionDto> Dishes { get; set; }
        public string Machine { get; set; }
        public string Session { get; set; }
        public string TransactionId { get; set; }
        public string BeginTranImage { get; set; }
        public string EndTranImage { get; set; }
    }
}