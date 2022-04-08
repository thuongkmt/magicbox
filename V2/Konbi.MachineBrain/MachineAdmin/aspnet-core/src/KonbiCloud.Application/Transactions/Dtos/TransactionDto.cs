using KonbiCloud.Enums;
using System;
using System.Collections.Generic;

namespace KonbiCloud.Transactions.Dtos
{
    using KonbiCloud.Inventories;

    public class TransactionDto
    {
        public TransactionDto()
        {
            Dishes = new List<DishTransactionDto>();
            Inventories = new List<InventoryItem>();
        }

        public long Id { get; set; }
        public string TranCode { get; set; }
        public string Buyer { get; set; }
        public DateTime PaymentTime { get; set; }
        public decimal? Amount { get; set; }
        public int PlatesQuantity { get; set; }
        public string States { get; set; }
        public ICollection<DishTransactionDto> Dishes { get; set; }
        public ICollection<InventoryItem> Inventories { get; set; }
        public int InventoriesQuantity { get; set; }
        public string Session { get; set; }
        public string TransactionId { get; set; }
        public string BeginTranImage { get; set; }
        public string EndTranImage { get; set; }
        public decimal? PaidAmount { get; set; }
        public string CardLabel { get; set; }
        public string CardNumber { get; set; }
        public string OtherInfo1 { get; set; }
        public bool IsSynced { get; set; }
    }
}