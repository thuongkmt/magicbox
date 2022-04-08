using System;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KonbiCloud.Machines;

namespace KonbiCloud.TemperatureLogs
{
    [Table("TemperatureLogs")]
    public class TemperatureLog : FullAuditedEntity, IMayHaveTenant
    {
        public int? TenantId { get; set; }

        [Required]
        public Guid MachineId { get; set; }

        public Machine Machine { get; set; }

        [Required]
        public decimal Temperature { get; set; }

        public TemperatureLog()
        {
            CreationTime = Clock.Now;
        }
    }
}
