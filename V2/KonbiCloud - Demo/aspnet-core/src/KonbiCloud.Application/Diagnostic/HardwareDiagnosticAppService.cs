using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Runtime.Caching;
using KonbiCloud.Common.Dtos;
using KonbiCloud.Diagnostic.Dtos;
//using KonbiCloud.Products.Dtos;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KonbiCloud.Diagnostic
{

    public class HardwareDiagnosticAppService : KonbiCloudAppServiceBase, IHardwareDiagnosticAppService
    {
        private readonly IRepository<HardwareDiagnostic, long> _hardwareDiagnosticRepository;
        private readonly IRepository<HardwareDiagnosticDetail, long> _hardwareDiagnosticDetailRepository;
        private readonly IUnitOfWorkManager _unitOfWork;
        private readonly ICacheManager _cacheManager;

        public HardwareDiagnosticAppService(IRepository<HardwareDiagnostic, long> hardwareDiagnosticRepository,
            IRepository<HardwareDiagnosticDetail, long> hardwareDiagnosticDetailRepository,
            IUnitOfWorkManager unitOfWork,
            ICacheManager cacheManager)
        {
            _hardwareDiagnosticRepository = hardwareDiagnosticRepository;
            _hardwareDiagnosticDetailRepository = hardwareDiagnosticDetailRepository;
            _unitOfWork = unitOfWork;
            _cacheManager = cacheManager;
        }
        [AbpAllowAnonymous]
        public async Task<bool> AddHardwareDiagnostic(HardwareDiagnosticFromClientDto input)
        {
            var diagnostic = input.MapTo<HardwareDiagnostic>();
            await _hardwareDiagnosticRepository.InsertAsync(diagnostic);
            await _unitOfWork.Current.SaveChangesAsync();
            foreach (var item in input.Details)
            {
                var detail = item.MapTo<HardwareDiagnosticDetail>();
                detail.HardwareDiagnosticId = diagnostic.Id;
                await _hardwareDiagnosticDetailRepository
                    .InsertAsync(detail);
            }
            await _unitOfWork.Current.SaveChangesAsync();
            return true;

        }

        public async Task<PagedResultDto<HardwareDiagnosticDto>> GetAllHardwareDiagnostic(HardwareDiagnosticInput input)
        {
            return await GetAllHardwareDiagnosticFromDb(input);

        }

        private async Task<PagedResultDto<HardwareDiagnosticDto>> GetAllHardwareDiagnosticFromDb(
            HardwareDiagnosticInput input)
        {

            if (input.MaxResultCount <= 0)
            {
                input.MaxResultCount = 20;
            }

            IQueryable<HardwareDiagnostic> query = _hardwareDiagnosticRepository
                .GetAll();

            if (!string.IsNullOrEmpty(input.MachineId))
            {
                query = query.Where(x => x.MachineId == input.MachineId);
            }
            if (input.Element != null)
            {
                query = query.Where(x => x.Element == input.Element);
            }
            if (input.FromDate != null && input.ToDate != null)
            {
                query = query.Where(x => x.OriginCreatedDate != null
                                         && x.OriginCreatedDate.Value.Date <= input.ToDate.Value.Date &&
                                         x.OriginCreatedDate.Value.Date >= input.FromDate.Value.Date);
            }
            var totalCount = query.Count();
            var results = query
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToList();

            return new PagedResultDto<HardwareDiagnosticDto>
            {
                TotalCount = totalCount,
                Items = results.MapTo<List<HardwareDiagnosticDto>>()
            };
        }
        public async Task<PagedResultDto<HardwareDiagnosticDetailDto>> GetHardwareDiagnosticDetail(long id, DateTime date)
        {
            var data = await _hardwareDiagnosticDetailRepository
                .GetAllListAsync(x => x.HardwareDiagnosticId == id && x.DateTime.Date == date.Date);
            var result = data
                .OrderBy(x => x.OriginId)
                .ToList();
            return new PagedResultDto<HardwareDiagnosticDetailDto>
            {
                Items = result.MapTo<List<HardwareDiagnosticDetailDto>>()
            };

        }
    }
}
