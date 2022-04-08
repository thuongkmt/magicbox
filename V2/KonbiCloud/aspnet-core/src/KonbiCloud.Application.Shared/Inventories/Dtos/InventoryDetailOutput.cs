using System;
using System.Collections.Generic;
using System.Linq;

namespace KonbiCloud.Inventories.Dtos
{
    public class InventoryDetailOutput
    {
        public InventoryDetailOutput()
        {
            InventoryDetailList = new List<InventoryDetailDto>();
        }

        public string MachineName { get; set; }

        public DateTime? TopupDate { get; set; }

        public int Total { get; set; }

        //public int CurrentStock => InventoryDetailList.Sum(x => x.CurrentStock);

        public int CurrentStock { get; set; }

        public int LeftOver => Total - Sold - Error;

        public int Sold => InventoryDetailList.Sum(x => x.Sold);

        public int Error { get; set; }
        public int Missing { get; set; }

        public ICollection<InventoryDetailDto> InventoryDetailList { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class InventoryDetailForRepportOutput
    {
        public InventoryDetailForRepportOutput()
        {
            InventoryDetailList = new List<InventoryDetailForReportDto>();
        }

        public string MachineName { get; set; }

        public DateTime? TopupDate { get; set; }

        public int Total { get; set; }

        //public int CurrentStock => InventoryDetailList.Sum(x => x.CurrentStock);

        public int CurrentStock { get; set; }

        public int LeftOver => Total - Sold - Error;

        public int Sold => InventoryDetailList.Sum(x => x.Sold);

        public int Error { get; set; }

        public ICollection<InventoryDetailForReportDto> InventoryDetailList { get; set; }
    }
}
