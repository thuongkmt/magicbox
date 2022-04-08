using Abp.Application.Services.Dto;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.TemperatureLogs.Dtos
{
    public class TemperatureLogListDto : FullAuditedEntity
    {
        public Guid MachineId { get; set; }

        public string MachineName { get; set; }

        public decimal Temperature { get; set; }
    }
}
