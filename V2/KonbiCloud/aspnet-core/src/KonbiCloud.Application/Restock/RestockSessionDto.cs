using System;
using System.Collections.Generic;

namespace KonbiCloud.Restock
{
    public class RestockSessionModelDto
    {
        public Guid Id { get; set; }
        public Guid MachineId { get; set; }
        public int Total { get; set; }
        public int LeftOver { get; set; }
        public int Sold { get; set; }
        public bool IsInprogress { get; set; }
    }
}
