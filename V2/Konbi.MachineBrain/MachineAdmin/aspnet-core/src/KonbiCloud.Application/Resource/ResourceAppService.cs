using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using KonbiCloud.Resources.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Resources
{
    public class ResourceAppService : KonbiCloudAppServiceBase, IResourceAppService
    {
        private readonly IRepository<Resource, Guid> _resourceRepository;

        public ResourceAppService(IRepository<Resource, Guid> resourceRepository)
        {
            _resourceRepository = resourceRepository;
        }

        public async Task<PagedResultDto<ResourceDto>> GetPagedList(GetAllResourceInput input)
        {
            try
            {
                var resources = await _resourceRepository.GetAll()
                    .OrderBy(x => x.CreationTime)
                    .PageBy(input)
                    .Select(x => new ResourceDto()
                    {
                        Id = x.Id,
                        FileName = x.FileName,
                        Thumbnail = x.Thumbnail
                    })
                    .ToListAsync();

                var totalCount = resources.Count;

                return new PagedResultDto<ResourceDto>()
                {
                    TotalCount = totalCount,
                    Items = resources
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Get all resources error", ex);
                return new PagedResultDto<ResourceDto>();
            }
        }

        public async Task<ListResultDto<ResourceDto>> GetAll()
        {
            try
            {
                var resources = await _resourceRepository.GetAll()
                    .OrderByDescending(x => x.CreationTime)
                    .Select(x => new ResourceDto()
                    {
                        Id = x.Id,
                        FileName = x.FileName,
                        Thumbnail = x.Thumbnail
                    })
                    .ToListAsync();

                return new ListResultDto<ResourceDto>(resources);
            }
            catch (Exception ex)
            {
                Logger.Error("Get all resources error", ex);
                return new ListResultDto<ResourceDto>();
            }
        }
    }
}
