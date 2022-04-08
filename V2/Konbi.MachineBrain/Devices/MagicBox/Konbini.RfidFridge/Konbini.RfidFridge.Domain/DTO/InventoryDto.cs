using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class TransactionDto
    {
        public CashlessDetailDto CashlessDetail { get; set; }
        public List<InventoryDto> Inventories { get; set; }
        public TransactionStatus Status { get; set; }
        public string PaymentDetail { get; set; }
    }
    public class CashlessDetailDto
    {
        public decimal Amount { get; set; }
        public string Tid { get; set; }
        public string Mid { get; set; }
        public string Invoice { get; set; }
        public string Batch { get; set; }
        public string CardLabel { get; set; }
        public string CardNumber { get; set; }
        public string Rrn { get; set; }
        public string ApproveCode { get; set; }
        public string EntryMode { get; set; }
        public string AppLabel { get; set; }
        public string Aid { get; set; }
        public string Tc { get; set; }
    }
    public class InventoryDto
    {
        public InventoryDto()
        {

        }

        public InventoryDto(string id, string tagId, int trayLevel, double price, ProductDto product)
        {
            Id = id;
            TagId = tagId;
            TrayLevel = trayLevel;
            Product = product;
            Price = price;
        }
        public string Id { get; set; }
        public string TagId { get; set; }
        public int TrayLevel { get; set; }
        public double Price { get; set; }
        public ProductDto Product { get; set; }
    }

    public class BlackListCardsDto
    {
        public BlackListCardsDto(string cardNumber, string cardLabel, decimal unpaidAmount)
        {
            CardNumber = cardNumber;
            CardLabel = cardLabel;
            UnpaidAmount = unpaidAmount;
        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("cardLabel")]
        public string CardLabel { get; set; }

        [JsonProperty("cardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty("unpaidAmount")]
        public decimal UnpaidAmount { get; set; }
    }

    public class UnstableInventoryDto
    {
        public InventoryDto Inventory { get; set; }
        public int NumberOfChanges { get; set; }
        public DateTime LastChange { get; set; }
    }

    public enum TransactionStatus
    {
        Success = 1,
        Cancelled = 2,
        ChangeError = 3,
        PaymentError = 4,
        Timeout = 5,
        Error = 6,
        Test = 7,
    }
}
