using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Domain.DTO
{
    public class ProductDto
    {
        public ProductDto()
        {

        }
        public ProductDto(string id, string name, string sku, double price)
        {
            this.Id = id;
            this.ProductName = name;
            this.Price = price;
            this.SKU = sku;
        }
        public string Id { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public double Price { get; set; }

        public override string ToString()
        {
            return ProductName;
        }
    }


    public class ProductSummarizeDto
    {
        public ProductSummarizeDto()
        {

        }
        public ProductSummarizeDto(string id, string name, int quantity)
        {
            this.Id = id;
            this.ProductName = name;
            this.Quantity = quantity;
        }
        public string Id { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }

    }
}
