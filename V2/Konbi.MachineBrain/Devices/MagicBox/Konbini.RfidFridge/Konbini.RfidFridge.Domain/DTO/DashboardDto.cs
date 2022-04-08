using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class DashboardDto
    {
        public DashboardDto(List<InventoryDto> currentInventory)
        {
            this.CurrentInventory = currentInventory;
            var results = from inven in CurrentInventory
                          group inven by inven.Product.Id into g
                          select new ProductSummarizeDto { Id = g.Key, ProductName = g.First().Product.ProductName, Quantity = g.Count() };

            ProductSummarize = results.ToList();
        }
        public DashboardDto()
        {

        }
        public List<InventoryDto> CurrentInventory { get; set; }
        public List<ProductSummarizeDto> ProductSummarize { get; set; }
        public double Temperature1 { get; set; }
        public double Temperature2 { get; set; }
    }
}
