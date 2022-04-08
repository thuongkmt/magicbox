using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.TopupReport.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.TopupReport
{
    public interface ITopupReportAppService: IApplicationService
    {
        Task<PagedResultDto<TopupListDto>> GetPagedList(TopupListInput input);

        Task<ListResultDto<TopupDetailDto>> GetDetail(Guid machineId);
    }
}
