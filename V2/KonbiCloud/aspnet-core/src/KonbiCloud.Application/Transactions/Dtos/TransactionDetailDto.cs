using KonbiCloud.Products;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Transactions.Dtos
{
    public class TransactionDetailDto
    {
        public TransactionDetailDto()
        {
            Product = new List<Product>();
        }
        public virtual string TagId { get; set; }

        public virtual double Price { get; set; }
        public virtual string TopupId { get; set; }
        public virtual string LocalInventoryId { get; set; }
        public string ProductName { get; set; }
        public ICollection<Product> Product { get; set; }
    }
}
