using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Inventories.Dtos
{
    public class GetCurrentTopupDto
    {
        public DateTime StartTime { get; set; }

        public int Total { get; set; }

        public int Sold { get; set; }

        public int LeftOver => Total - Sold;
        public int RestockSessionId { get; set; }
    }
}
