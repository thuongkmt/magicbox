using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Inventories.Dtos
{
    public class InventoryDetailForReportDto
    {
        public DateTime? LastSoldDate { get; set; }
        public string ProductName { get; set; }
        public string ProductCategory { get; set; }
        public string ProductSku { get; set; }
        public string ProductExpiredDate { get; set; }
        public string ProductDescription { get; set; }
        public double? ProductPrice { get; set; }

        public int CurrentStock { get; set; }
        public int Total { get; set; }
        public int Sold { get; set; }
        public int Error { get; set; }
        public int Missing { get; set; }

        public int LeftOver => Total - Sold - Error;
    }
}
