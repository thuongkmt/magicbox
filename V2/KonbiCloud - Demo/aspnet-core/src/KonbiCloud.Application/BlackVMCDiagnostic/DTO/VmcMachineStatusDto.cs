using System;
using Abp.Application.Services.Dto;

namespace KonbiCloud.BlackVMCDiagnostic.Dtos
{
    public class VmcMachineStatusDto : FullAuditedEntityDto<Guid>
    {
        public string MachineId { get; set; }
        public DateTime LogTime { get; set; }

        public string A0 { get; set; }
        public string A1 { get; set; }
        public string A2 { get; set; }
        public string A3 { get; set; }
        public string A4 { get; set; }
        public string A5 { get; set; }
        public string A6 { get; set; }
        public string A7 { get; set; }
        public string A8 { get; set; }
        public string A9 { get; set; }

        public string B0 { get; set; }
        public string B1 { get; set; }
        public string B2 { get; set; }
        public string B3 { get; set; }
        public string B4 { get; set; }
        public string B5 { get; set; }
        public string B6 { get; set; }
        public string B7 { get; set; }
        public string B8 { get; set; }
        public string B9 { get; set; }

        public string C0 { get; set; }
        public string C1 { get; set; }
        public string C2 { get; set; }
        public string C3 { get; set; }
        public string C4 { get; set; }
        public string C5 { get; set; }
        public string C6 { get; set; }
        public string C7 { get; set; }
        public string C8 { get; set; }
        public string C9 { get; set; }

        public string D0 { get; set; }
        public string D1 { get; set; }
        public string D2 { get; set; }
        public string D3 { get; set; }
        public string D4 { get; set; }
        public string D5 { get; set; }
        public string D6 { get; set; }
        public string D7 { get; set; }
        public string D8 { get; set; }
        public string D9 { get; set; }

        public string E0 { get; set; }
        public string E1 { get; set; }
        public string E2 { get; set; }
        public string E3 { get; set; }
        public string E4 { get; set; }
        public string E5 { get; set; }
        public string E6 { get; set; }
        public string E7 { get; set; }
        public string E8 { get; set; }
        public string E9 { get; set; }

        public string F0 { get; set; }
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public string F6 { get; set; }
        public string F7 { get; set; }
        public string F8 { get; set; }
        public string F9 { get; set; }
    }
}