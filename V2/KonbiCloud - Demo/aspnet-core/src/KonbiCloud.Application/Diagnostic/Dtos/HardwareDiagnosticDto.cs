using System;
using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace KonbiCloud.Diagnostic.Dtos
{
    [AutoMapFrom(typeof(HardwareDiagnostic))]
    public class HardwareDiagnosticDto : FullAuditedEntityDto<long>
    {
        public int TenantId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Element { get; set; }
        public string MachineName { get; set; }
        public string MachineId { get; set; }
        public DateTime? OriginCreatedDate { get; set; }
        public long OriginId { get; set; }

    }
    public class HardwareDiagnosticFromClientDto
    {
        public long Id { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Element { get; set; }
        public string MachineName { get; set; }
        public string MachineId { get; set; }
        public DateTime? CreatedDate { get; set; }

        public IEnumerable<HardwareDiagnosticDetailDto> Details { get; set; }
    }
}
