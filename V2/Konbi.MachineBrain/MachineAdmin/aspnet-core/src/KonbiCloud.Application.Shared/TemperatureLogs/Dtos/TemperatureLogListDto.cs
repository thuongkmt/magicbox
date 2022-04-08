using System;
using Abp.Domain.Entities.Auditing;

namespace KonbiCloud.TemperatureLogs.Dtos
{
    public class TemperatureLogListDto : FullAuditedEntity
    {
        public decimal Temperature { get; set; }
        public bool IsSynced { get; set; }
        public DateTime? SyncDate { get; set; }
    }
}
