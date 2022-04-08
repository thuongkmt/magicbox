using Konbini.Messages.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KonbiCloud.RFIDTable
{
    public class TransactionInfo
    {
        public Guid Id { get; set; }
        public List<MenuItemInfo> MenuItems { get; set; }
        public decimal Amount => (MenuItems != null ? MenuItems.Sum(el => el.Price) : (decimal)0.0);
        public int PlateCount => MenuItems.Count();
        public PaymentState PaymentState { get; set; }
        public PaymentType PaymentType { get; set; }
        public string CustomerMessage { get; set; }
        public Guid SessionId { get; set; }
        public string Buyer { get; set; }
        public string BeginTranImage { get; set; }
        public string EndTranImage { get; set; }
    }
}
