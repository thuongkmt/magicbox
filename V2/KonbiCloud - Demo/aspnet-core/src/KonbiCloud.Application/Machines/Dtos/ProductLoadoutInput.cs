using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Machines.Dtos
{
    public class ProductLoadoutInput
    {
        public Guid MachineId { get; set; }
        public Guid SessionId { get; set; }
        public Guid? ProductId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int LocationCode { get; set; }
    }
}
