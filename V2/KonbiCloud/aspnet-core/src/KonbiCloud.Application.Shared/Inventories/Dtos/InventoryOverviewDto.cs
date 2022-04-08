using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Inventories.Dtos
{
    public class InventoryOverviewDto
    {
        public Guid MachineId { get; set; }

        public Guid TopupId { get; set; }

        public string MachineName { get; set; }

        public DateTime? TopupDate { get; set; }

        public int Total { get; set; }

        public int LeftOver => Total - Sold - Error;

        public int CurrentStock { get; set; }

        public int Sold { get; set; }

        public int Error { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
