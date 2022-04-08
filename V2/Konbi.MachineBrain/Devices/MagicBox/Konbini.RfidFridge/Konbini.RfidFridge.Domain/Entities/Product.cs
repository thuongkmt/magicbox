using Konbini.RfidFridge.Domain.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Konbini.RfidFridge.Domain.Entities
{
    public class Product : AuditableEntity<long>
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string Sku { get; set; }
    }
}
