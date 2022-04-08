using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using KonbiCloud.Authorization;
using KonbiCloud.Inventories;
using KonbiCloud.TopupReport.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace KonbiCloud.TopupReport
{
    [AbpAuthorize(AppPermissions.Pages_Reports)]
    public class TopupReportAppService : KonbiCloudAppServiceBase, ITopupReportAppService
    {
        private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
        private readonly IRepository<Topup, Guid> _topupRepository;
        public TopupReportAppService(IRepository<InventoryItem, Guid> inventoryRepository,
                                     IRepository<Topup, Guid> topupRepository)
        {
            _inventoryRepository = inventoryRepository;
            _topupRepository = topupRepository;
        }

        public async Task<ListResultDto<TopupDetailDto>> GetDetail(Guid topupId)
        {
            try
            {
                var iv = _inventoryRepository.GetAllIncluding()
                        .Where(x => x.TopupId == topupId)
                        .Include(x => x.Product);
                var group = iv.GroupBy(x => x.Product.Name);

                var detailList = await _inventoryRepository.GetAllIncluding()
                        .Where(x => x.TopupId == topupId)
                        .Include(x => x.Product)
                        .GroupBy(x => x.Product.Name)
                        .Select(x => new TopupDetailDto {
                            ProductName = x.Key,
                            Total = x.Count(),
                            Sold = x.Count(y => y.DetailTransactionId.HasValue),
                            SalesAmount = (decimal)x.Where(y => y.DetailTransactionId.HasValue).Sum(y => y.Price)
                        })
                        .OrderBy(x => x.ProductName)
                        .ToListAsync();

                return new ListResultDto<TopupDetailDto>(detailList);
            }
            catch(Exception ex)
            {
                Logger.Error("Error when get Topup detail", ex);
            }

            return new ListResultDto<TopupDetailDto>();
        }

        public async Task<PagedResultDto<TopupListDto>> GetPagedList(TopupListInput input)
        {
            try
            {
                var topups = _topupRepository.GetAll()
                .WhereIf(input.StartTime != null, x => x.StartDate >= input.StartTime)
                .WhereIf(input.EndTime != null, x => x.EndDate < input.EndTime.Value.AddDays(1));

                var totalCount = await topups.CountAsync();
                var returnTopups = await topups.OrderBy(input.Sorting ?? "startDate desc").PageBy(input).ToListAsync();

                var result = new List<TopupListDto>();
                foreach (var topup in returnTopups)
                {
                    var soldItems = _inventoryRepository.GetAll().Where(x => x.TopupId == topup.Id && x.DetailTransactionId.HasValue);
                    result.Add(new TopupListDto()
                    {
                        Id = topup.Id,
                        StartTime = topup.StartDate,
                        EndTime = topup.EndDate,
                        Total = topup.Total,
                        Sold = soldItems.Count(x => x.DetailTransactionId.HasValue),
                        Errors = topup.Error,
                        SalesAmount = (decimal)soldItems.Sum(y => y.Price)
                    });
                }

                return new PagedResultDto<TopupListDto>()
                {
                    TotalCount = totalCount,
                    Items = result.ToList()
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                return new PagedResultDto<TopupListDto>()
                {
                    TotalCount = 0,
                    Items = null
                };
            }
        }
    }
}
