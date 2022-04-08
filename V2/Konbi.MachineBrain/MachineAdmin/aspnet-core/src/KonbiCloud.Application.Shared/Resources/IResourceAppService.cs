using Abp.Application.Services;
using Abp.Application.Services.Dto;
using KonbiCloud.Resources.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Resources
{
    public interface IResourceAppService : IApplicationService
    {
        Task<PagedResultDto<ResourceDto>> GetPagedList(GetAllResourceInput input);

        Task<ListResultDto<ResourceDto>> GetAll();
    }
}
