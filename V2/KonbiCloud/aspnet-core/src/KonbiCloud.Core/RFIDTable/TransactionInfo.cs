using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KonbiCloud.RFIDTable
{
    public class TransactionInfo
    {
        public Guid Id { get; set; }
        public List<MenuItemInfo> MenuItems { get; set; }
        public decimal Amount => (MenuItems != null ? MenuItems.Sum(el => el.Price) : (decimal)0.0);
        public int PlateCount => MenuItems.Count();
    }
}
