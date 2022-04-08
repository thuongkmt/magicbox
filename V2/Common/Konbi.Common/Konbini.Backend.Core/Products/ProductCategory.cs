using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Abp.Domain.Entities;
using Newtonsoft.Json;

namespace KonbiCloud.Products
{
    public class ProductCategory: Entity<long>
    {
        [JsonIgnore]
        public virtual Product Product { get; set; }
        public virtual Category Category { get; set; }
        [ForeignKey("Category")]

        public Guid CategoryId { get; set; }
    }
}
