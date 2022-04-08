using System;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KonbiCloud.TemperatureLogs
{
    [Table("TemperatureLogs")]
    public class TemperatureLog : FullAuditedEntity, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [Required]
        public decimal Temperature { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncDate { get; set; }
        public TemperatureLog()
        {
            CreationTime = DateTime.Now;
        }
    }
}
