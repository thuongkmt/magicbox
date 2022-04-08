using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using KonbiCloud.Machines;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KonbiCloud.Inventories
{
    [Table("CurrentInventory")]
    public class CurrentInventory: AuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public Guid MachineId { get; set; }
        [ForeignKey("MachineId")] 
        public virtual Machine Machine { get; set; }
        public string Tag { get; set; }
        public string ProductName { get; set; }
        
    }
}
