using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.TagManagement.DTO
{
    public class ProductDTO : BaseAbpDTO<ProductDTO.Product>
    {
        public class Item
        {
            [JsonProperty("product")]
            public Product Product { get; set; }
        }

        public class Product
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("sku")]
            public string Sku { get; set; }

            [JsonProperty("barcode")]
            public string Barcode { get; set; }

            [JsonProperty("shortDesc")]
            public object ShortDesc { get; set; }

            [JsonProperty("desc")]
            public object Desc { get; set; }

            [JsonProperty("tag")]
            public string Tag { get; set; }

            [JsonProperty("imageUrl")]
            public string ImageUrl { get; set; }

            [JsonProperty("price")]
            public double Price { get; set; }

            [JsonProperty("creationTime")]
            public DateTime CreationTime { get; set; }

            [JsonProperty("categoriesName")]
            public List<object> CategoriesName { get; set; }

            [JsonProperty("id")]
            public Guid Id { get; set; }
        }

    }


   
}
