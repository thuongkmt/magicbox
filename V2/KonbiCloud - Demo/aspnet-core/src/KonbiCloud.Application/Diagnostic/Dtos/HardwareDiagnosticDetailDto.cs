using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace KonbiCloud.Diagnostic.Dtos
{
    [AutoMapFrom(typeof(HardwareDiagnosticDetail))]
    public class HardwareDiagnosticDetailDto : FullAuditedEntityDto<long>
    {
        public int TenantId { get; set; }
        public string Detail { get; set; }
        public DateTime DateTime { get; set; }
        public long HardwareDiagnosticId { get; set; }
        public int Level { get; set; }
    }
}
