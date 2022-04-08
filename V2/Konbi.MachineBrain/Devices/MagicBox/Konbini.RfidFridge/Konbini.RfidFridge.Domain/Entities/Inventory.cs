using Konbini.RfidFridge.Domain.Base;

namespace Konbini.RfidFridge.Domain.Entities
{
    using Konbini.RfidFridge.Domain.Entities;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Inventory : AuditableEntity<long>
    {
        public long ProductId { get; set; }

        public string TagId { get; set; }
        public string TrayLevel { get; set; }
        public float Price { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
