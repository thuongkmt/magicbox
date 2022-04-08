using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using KonbiCloud.Diagnostic.Dtos;

namespace KonbiCloud.Diagnostic
{
    public interface IMachineDiagnosticAppService
    {
        Task<List<VmcHistoryDto>> VmcHistory();
        Task<ListResultDto<MachineErrorDto>> GetAllMachineErrors(PagedAndSortedResultRequestDto input);

    }
}
