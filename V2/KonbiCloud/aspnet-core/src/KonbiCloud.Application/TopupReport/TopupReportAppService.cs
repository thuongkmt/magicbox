using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.Enums;
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
        private readonly IRepository<TopupHistory, long> _topupHistoryRepository;

        public TopupReportAppService(IRepository<InventoryItem, Guid> inventoryRepository,
              IRepository<Topup, Guid> topupRepository, IRepository<TopupHistory, long> topupHistoryRepository)
        {
            _inventoryRepository = inventoryRepository;
            _topupRepository = topupRepository;
            _topupHistoryRepository = topupHistoryRepository;
        }

        public async Task<PagedResultDto<TopupCsvDto>> GetDataForReport(TopupListInput input)
        {
            try
            {
                var masterData = await GetPagedList(input);

                var result = new List<TopupCsvDto>();
                //var totalCount = 0;

                foreach (var data in masterData.Items)
                {
                    var id = data.Id;
                    var prefix = data.Type == Enums.TopupTypeEnum.Restock ? "RE" : "UN";
                    var machineName = data.MachineName;
                    var itemPrefix = $"{machineName}_{prefix}_{id}";
                    var topupType = data.Type;

                    var topupDetail = await GetDetailForReport(id, topupType);
                    var topupDetailItems = new ListResultDto<TopupDetailDto>();
                    if (topupType == Enums.TopupTypeEnum.Restock)
                    {
                        topupDetailItems.Items = topupDetail.Items.Where(x => x.Type == TopUpInventoryTypeEnum.NEW_PRODUCT).ToList();
                    }
                    if (topupType == Enums.TopupTypeEnum.Unload)
                    {
                        topupDetailItems.Items = topupDetail.Items.Where(x => x.Type == TopUpInventoryTypeEnum.THROW_OUT_PRODUCT).ToList();
                    }
                    foreach (var detail in topupDetailItems.Items)
                    {

                        result.Add(new TopupCsvDto
                        {
                            // Master data
                            MachineId = machineName,
                            ItemId = itemPrefix,
                            RestockerId = data.RestockerName,
                            DateTime = data.StartTime,
                            SessionType = data.Type.ToString(),

                            // Detail items
                            ItemName = detail.ProductName,
                            Category = "",
                            Quantity = detail.Total,
                            QuantityBefore = 0,
                            QuantityFinal = 0,
                            SKU = ""
                        });
                    }

                }

                return new PagedResultDto<TopupCsvDto>()
                {
                    TotalCount = result.Count,
                    Items = result
                };
            }
            catch (Exception ex)
            {
                Logger.Error("Error", ex);
                return new PagedResultDto<TopupCsvDto>()
                {
                    TotalCount = 0,
                    Items = null
                };
            }
        }


        public async Task<ListResultDto<TopupDetailDto>> GetDetailForReport(Guid topupId, TopupTypeEnum type)
        {
            try
            {
                var result = new List<TopupDetailDto>();
                var currentTopup = _topupRepository.Get(topupId);
                if (currentTopup == null) return new ListResultDto<TopupDetailDto>();

                var previousTopup = await _topupRepository.GetAll().Where(x => x.MachineId == currentTopup.MachineId && x.StartDate < currentTopup.StartDate).OrderByDescending(x => x.StartDate).FirstOrDefaultAsync();

                if (previousTopup != null)
                {
                    //Get all tags from previous topup session
                    var oldInventoryTagIds = await _topupHistoryRepository.GetAll().Include(x => x.InventoryItem).Where(x => x.TopupId == previousTopup.Id).Select(x => x.InventoryItem.TagId).ToListAsync();

                    var currentTopupInventoryTags = await _topupHistoryRepository.GetAll().Include(x => x.InventoryItem).Where(x => x.TopupId == topupId).Select(x => x.InventoryItem.TagId).ToListAsync();

                    


                    if (type == TopupTypeEnum.Restock)
                    {
                        //Then get all new inventories
                        var newInventories = await _topupHistoryRepository.GetAll()
                            .Include(x => x.InventoryItem)
                            .ThenInclude(x => x.Product)
                            .Where(x => x.TopupId == topupId && !oldInventoryTagIds.Contains(x.InventoryItem.TagId))
                            .GroupBy(x => x.InventoryItem.Product.Name)
                            .Select(x => new TopupDetailDto
                            {
                                ProductName = x.Key,
                                Total = x.Count(),
                                Sold = x.Count(y => y.InventoryItem.DetailTransactionId.HasValue && y.InventoryItem.Transaction.Status == Enums.TransactionStatus.Success),
                                SalesAmount = (decimal)x.Where(y => y.InventoryItem.DetailTransactionId.HasValue && y.InventoryItem.Transaction.Status == Enums.TransactionStatus.Success).Sum(y => y.InventoryItem.Price),
                                Type = TopUpInventoryTypeEnum.NEW_PRODUCT
                            })
                            .OrderBy(x => x.ProductName)
                            .ToListAsync();

                        // result.AddRange(oldInventories);
                        result.AddRange(newInventories);
                    }

                    if (type == TopupTypeEnum.Unload)
                    {

                        //Then get all taken out inventories : exist in previous topup session, was not sold but not exist in current session
                        var takenOutInventories = await _topupHistoryRepository.GetAll()
                        .Include(x => x.InventoryItem)
                        .ThenInclude(x => x.Product)
                        .Where(x => (x.TopupId == previousTopup.Id && x.InventoryItem.DetailTransactionId == null && !currentTopupInventoryTags.Contains(x.InventoryItem.TagId))
                        || (x.TopupId == topupId && currentTopup.Type == Enums.TopupTypeEnum.Unload && x.InventoryItem.State == Enums.TagState.Unloaded))
                        .GroupBy(x => x.InventoryItem.Product.Name)
                        .Select(x => new TopupDetailDto
                        {
                            ProductName = x.Key,
                            Total = x.Count(),
                            Sold = x.Count(y => y.InventoryItem.DetailTransactionId.HasValue && y.InventoryItem.Transaction.Status == Enums.TransactionStatus.Success),
                            SalesAmount = (decimal)x.Where(y => y.InventoryItem.DetailTransactionId.HasValue && y.InventoryItem.Transaction.Status == Enums.TransactionStatus.Success).Sum(y => y.InventoryItem.Price),
                            Type = TopUpInventoryTypeEnum.THROW_OUT_PRODUCT
                        })
                        .OrderBy(x => x.ProductName)
                        .ToListAsync();

                        result.AddRange(takenOutInventories);
                    }
                }
                else
                {
                    result = await _topupHistoryRepository.GetAll()
                                    .Include(x => x.InventoryItem)
                                    .ThenInclude(x => x.Product)
                                    .Where(x => x.TopupId == topupId)
                                    .GroupBy(x => x.InventoryItem.Product.Name)
                                    .Select(x => new TopupDetailDto
                                    {
                                        ProductName = x.Key,
                                        Total = x.Count(),
                                        Sold = x.Count(y => y.InventoryItem.DetailTransactionId.HasValue && y.InventoryItem.Transaction.Status == Enums.TransactionStatus.Success),
                                        SalesAmount = (decimal)x.Where(y => y.InventoryItem.DetailTransactionId.HasValue && y.InventoryItem.Transaction.Status == Enums.TransactionStatus.Success).Sum(y => y.InventoryItem.Price),
                                        Type = TopUpInventoryTypeEnum.NONE
                                    })
                                    .OrderBy(x => x.ProductName)
                                    .ToListAsync();
                }

                return new ListResultDto<TopupDetailDto>(result);
            }
            catch (Exception ex)
            {
                Logger.Error("Error when get Topup detail", ex);
            }

            return new ListResultDto<TopupDetailDto>();
        }

        public async Task<ListResultDto<TopupDetailDto>> GetDetail(Guid topupId)
        {
            try
            {

                var topupSession = await _topupRepository.GetAsync(topupId);
                if(topupSession == null)
                {
                    throw new UserFriendlyException($"No restock session found with Id: {topupId}");
                }           
                var historyInventory = _topupHistoryRepository.GetAll()
                        .Include(x => x.InventoryItem)
                        .ThenInclude(x => x.Product)
                        .Where(el => el.TopupId == topupSession.Id).Select(el => new { 
                            ProductName = !string.IsNullOrEmpty(el.ProductName)? el.ProductName: (el.InventoryItem!=null? (el.InventoryItem.Product!=null?el.InventoryItem.Product.Name: ""): ""),
                            el.Tag,
                            el.Price,
                            el.Type,                            
                        }).ToList();
                var output = new List<TopupDetailDto>();
                historyInventory.Where(el=> el.Type == TopupHistoryType.Current).GroupBy(el=> el.ProductName).ToList().ForEach(el => {
                    output.Add(new TopupDetailDto() { 
                        ProductName = el.Key,
                        Total = el.Count(),
                        Type = TopUpInventoryTypeEnum.OLD_PRODUCT
                    });
                });
                historyInventory.Where(el => el.Type == TopupHistoryType.Stocked).GroupBy(el => el.ProductName).ToList().ForEach(el => {
                    output.Add(new TopupDetailDto()
                    {
                        ProductName = el.Key,
                        Total = el.Count(),
                        Type = TopUpInventoryTypeEnum.NEW_PRODUCT
                    });
                });
                historyInventory.Where(el => el.Type == TopupHistoryType.Unloaded).GroupBy(el => el.ProductName).ToList().ForEach(el => {
                    output.Add(new TopupDetailDto()
                    {
                        ProductName = el.Key,
                        Total = el.Count(),
                        Type = TopUpInventoryTypeEnum.THROW_OUT_PRODUCT
                    });
                });

                return new ListResultDto<TopupDetailDto>(output);
            }
            catch (Exception ex)
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
                .Include(x => x.Machine)
                .WhereIf(input.MachineId.HasValue, x => x.MachineId == input.MachineId)
                .WhereIf(input.StartTime.HasValue, x => x.StartDate >= input.StartTime || (x.EndDate.HasValue && x.EndDate>=input.StartTime))
                .WhereIf(input.EndTime.HasValue, x => x.StartDate <= input.EndTime || (x.EndDate.HasValue && x.EndDate<=input.EndTime));

                var totalCount = await topups.CountAsync();
                var result = await topups.OrderBy(input.Sorting ?? "startDate desc").PageBy(input).ToListAsync();

                var output = new List<TopupListDto>();
                foreach (var topup in result)
                {
                    var soldItems = _inventoryRepository.GetAll()
                        .Where(x => x.TopupId == topup.Id &&
                                    x.DetailTransactionId.HasValue &&
                                    x.Transaction.Status == Enums.TransactionStatus.Success)
                        .Select(el=> new { el.Id, el.Price });
                    DateTime? endDate = null;
                    if (topup.EndDate.HasValue)
                    {
                        endDate = topup.EndDate.Value;
                    }

                    output.Add(new TopupListDto()
                    {
                        MachineId = topup.MachineId,
                        Id = topup.Id,
                        MachineName = topup.Machine.Name,
                        StartTime = topup.StartDate,
                        EndTime = endDate,
                        Total = topup.Total,
                        Sold = soldItems.Count(),
                        Errors = topup.Error,
                        SalesAmount = (decimal)soldItems.Sum(y => y.Price),
                        RestockerName = topup.RestockerName,
                        Type = topup.Type
                    });
                }

                return new PagedResultDto<TopupListDto>()
                {
                    TotalCount = totalCount,
                    Items = output.ToList()
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
