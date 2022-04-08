using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class OrderDto
    {
        public double Amount;
        public List<InventoryDto> Inventories;

        public OrderStatus Status;
    }

    public enum OrderStatus
    {
        Processing,
        Done,
        Cancelled
    }
}
