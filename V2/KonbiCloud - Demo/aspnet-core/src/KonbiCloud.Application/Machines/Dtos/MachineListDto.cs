using System;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace KonbiCloud.Machines.Dtos
{

    [AutoMapFrom(typeof(Machine))]
    public class MachineListDto : FullAuditedEntityDto<Guid>
    {
        public string Name { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public string CashlessTerminalId { get; set; }
        public bool RegisteredAzureIoT { get; set; }
    }
}
