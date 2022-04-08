using KonbiCloud.Enums;
using KonbiCloud.Inventories;
using System;
using System.Collections.Generic;

namespace KonbiCloud.Transactions.Dtos
{
    public class TransactionFinanceReportDto
    {
        public string Machine { get; set; }
        public string TransactionId { get; set; }
        public DateTime DateTime { get; set; }
        public int Quantity { get; set; }
        public string TransactionStatus { get; set; }
        public string PaymentType { get; set; }
        public string CardId { get; set; }
        public decimal DepositCollected { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountRefunded { get; set; }
    }

}

