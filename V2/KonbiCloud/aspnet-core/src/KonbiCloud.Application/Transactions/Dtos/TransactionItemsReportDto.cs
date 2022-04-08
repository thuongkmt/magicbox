using KonbiCloud.Enums;
using KonbiCloud.Inventories;
using System;
using System.Collections.Generic;

namespace KonbiCloud.Transactions.Dtos
{
 
    public class TransactionItemsReportDto
    {
        public string Machine { get; set; }
        public string TransactionId { get; set; }
        public DateTime DateTime { get; set; }
        public string Category { get; set; }
        public string ProductName { get; set; }
        public string ExpireDate { get; set; }
        public string Sku { get; set; }
        public string TagId { get; set; }
        public double? ProductUnitPrice { get; set; }
        public double? ProductDiscountPrice { get;set; }
        public string TransactionStatus { get; set; }
        public string PaymentType { get; set; }
        public string CardId { get; set; }
        public decimal AmountPaid { get; set; }
    }

}

