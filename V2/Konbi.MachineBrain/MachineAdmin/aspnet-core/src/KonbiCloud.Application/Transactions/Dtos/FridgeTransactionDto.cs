using KonbiCloud.Enums;
using System;
using System.Collections.Generic;

namespace KonbiCloud.Transactions.Dtos
{
    public class FridgeTransactionDto
    {
        //public long Id { get; set; }
        //public string TranCode { get; set; }
        //public string Buyer { get; set; }
        //public DateTime PaymentTime { get; set; }
        //public decimal? Amount { get; set; }
        //public int PlatesQuantity { get; set; }
        //public string States { get; set; }
        //public ICollection<DishTransactionDto> Dishes { get; set; }
        //public string Session { get; set; }
        //public string TransactionId { get; set; }
        //public string BeginTranImage { get; set; }
        //public string EndTranImage { get; set; }
        public FridgeTransactionDto()
        {

        }
        public CashlessDetail CashlessDetail { get; set; }
        public List<FridgeTransactionInventoryDto> Inventories { get; set; }
        public TransactionStatus Status { get; set; }
        public string PaymentDetail { get; set; }
    }


    public class FridgeTransactionInventoryDto
    {
        public FridgeTransactionInventoryDto()
        {

        }

        public Guid Id { get; set; }
        public string TagId { get; set; }
        public int TrayLevel { get; set; }
        public double Price { get; set; }
        public FridgeTransactionProductDto Product { get; set; }
    }

    public class FridgeTransactionProductDto
    {

        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public double Price { get; set; }

        public override string ToString()
        {
            return ProductName;
        }
    }
}