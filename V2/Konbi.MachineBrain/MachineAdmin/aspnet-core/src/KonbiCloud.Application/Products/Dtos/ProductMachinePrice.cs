using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Products.Dtos
{
    public class ProductMachinePrice
    {
        public Guid MachineId { get; set; }

        public Guid ProductId { get; set; }

        public decimal Price { get; set; }
    }
}
