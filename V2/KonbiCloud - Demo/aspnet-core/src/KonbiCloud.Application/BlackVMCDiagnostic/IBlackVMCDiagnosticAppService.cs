using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.BlackVMCDiagnostic.Dtos;

namespace KonbiCloud.BlackVMCDiagnostic
{
    public interface IBlackVMCDiagnosticAppService : IApplicationService
    {
        Task<ListResultDto<BlackVMCDiagnosticDto>> GetLevelDiagnostic(int maxResultCount, int skipCount, string machineId);
        Task<VmcMachineStatusDto> GetMachineStatus(string machineId);
        Task<bool> AddLevelDiagnostic(VMCDiagnostic vmc);
        Task<bool> AddOrUpdateMachineStatus(MachineStatus mc);
    }
}
