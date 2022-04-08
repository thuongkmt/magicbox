using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Inventories.Dtos
{
    public class InventoryDetailDto
    {
        public DateTime? LastSoldDate { get; set; }
        public string ProductName { get; set; }
        public int CurrentStock { get; set; }
        public int Total { get; set; }
        public int Sold { get; set; }
        public int Error { get; set; }
        public int Missing { get { return Total - CurrentStock - Sold; } }
        public string Type { get; set; }
        public int LeftOver => Total - Sold - Error;
    }
}
