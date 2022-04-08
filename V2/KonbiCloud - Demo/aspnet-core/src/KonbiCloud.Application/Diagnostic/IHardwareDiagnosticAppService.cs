using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using KonbiCloud.Diagnostic.Dtos;

namespace KonbiCloud.Diagnostic
{
    public interface IHardwareDiagnosticAppService
    {
        Task<bool> AddHardwareDiagnostic(HardwareDiagnosticFromClientDto input);
        Task<PagedResultDto<HardwareDiagnosticDto>> GetAllHardwareDiagnostic(HardwareDiagnosticInput input);
        Task<PagedResultDto<HardwareDiagnosticDetailDto>> GetHardwareDiagnosticDetail(long id, DateTime date);
    }
}
