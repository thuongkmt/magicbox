using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Uow;
using Abp.MultiTenancy;
using KonbiCloud.Authorization;
using KonbiCloud.Common;
using KonbiCloud.EntityFrameworkCore;
using KonbiCloud.Inventories;
using KonbiCloud.Machines;
using KonbiCloud.Machines.Dtos;
using KonbiCloud.Restock;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using System.Transactions;

namespace KonbiCloud.MachineLoadout
{
    [AbpAuthorize(AppPermissions.Pages_Machines)]

    public class MachineLoadoutAppService : KonbiCloudAppServiceBase, IMachineLoadoutAppService
    {
        private readonly IRepository<LoadoutItem, Guid> _loadoutItemRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private IIocResolver _iocResolver;
        private readonly IDetailLogService _detailLogService;
        private readonly IRepository<Topup, Guid> _restockSessionRepository;
        private readonly IRepository<RestockSessionHistory, Guid> _restockSessionHistoryRepository;

        public MachineLoadoutAppService(
                                        IRepository<LoadoutItem, Guid> loadoutItemRepository,
                                        IRepository<Machine, Guid> machineRepository,
                                        IIocResolver iocResolver,
                                        IDetailLogService detailLog,
                                        IRepository<Topup, Guid> restockSessionRepository,
                                        IRepository<RestockSessionHistory, Guid> restockSessionHistoryRepository)
        {
            _loadoutItemRepository = loadoutItemRepository;
            _machineRepository = machineRepository;
            _iocResolver = iocResolver;
            _detailLogService = detailLog;
            _restockSessionRepository = restockSessionRepository;
            _restockSessionHistoryRepository = restockSessionHistoryRepository;
        }
        public async Task<LoadoutDto> GetLoadout(EntityDto<Guid> machine)
        {
            var dto = new LoadoutDto
            {
                MachineId = machine.Id,
                LoadoutList = new List<LoadoutList>(),
                IsOnline = true
            };
            try
            {
                var mc = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machine.Id);
                if (mc == null) return dto;

                dto.MachineName = mc.Name;
                var loadouts = await _loadoutItemRepository.GetAll()
                                        .Include(x => x.Product)
                                        .Where(x => x.Machine.Id == machine.Id)
                                        .OrderBy(x => x.ItemLocation).ToListAsync();

                if (loadouts == null) return dto;

                if (mc.MachineType == "Vending")
                {
                    foreach (var lo in loadouts)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            if (!string.IsNullOrEmpty(lo.ItemLocation) && lo.ItemLocation.StartsWith(i.ToString()))
                            {
                                if (!dto.LoadoutList.Any(x => x.Index == i))
                                {
                                    dto.LoadoutList.Add(new LoadoutList { Index = i, Loadouts = new List<LoadoutItemDto>() });
                                }
                                var item = dto.LoadoutList.FirstOrDefault(x => x.Index == i);
                                if (item != null && item.Loadouts != null)
                                {
                                    var lid = lo.MapTo<LoadoutItemDto>();
                                    if (lid.ProductId == Guid.Empty)
                                    {
                                        lid.ProductId = null;
                                    }
                                    item.Loadouts.Add(lid);
                                }
                            }
                        }
                    }
                }
                return dto;
            }
            catch (Exception ex)
            {
                Logger.Error($"Get all loadout error:{ex.Message}", ex);
                return dto;
            }
            
        }

        public async Task<bool> UpdateLoadout(UpdateLoadoutDto input)
        {
            _detailLogService.Log($"Cloud - UpdateLoadout - received data: {input.MachineId} / {input.Loadouts.Count}");
            try
            {
                var currentSession = await _restockSessionRepository.FirstOrDefaultAsync(x => x.MachineId == input.MachineId && !x.EndDate.HasValue);
                if (currentSession == null)
                {
                    _detailLogService.Log($"Cloud - UpdateLoadout - cannot find Restock Session");
                }

                var loadouts = await _loadoutItemRepository.GetAll()
                                        .Where(x => x.Machine.Id == input.MachineId)
                                        .OrderBy(x => x.ItemLocation).ToListAsync();

                var updatedLoadouts = new List<LoadoutItem>();
                var newHistories = new List<RestockSessionHistory>();
                foreach (var lo in input.Loadouts)
                {
                    _detailLogService.Log($"Cloud - UpdateLoadout - input loadout: {JsonConvert.SerializeObject(lo)}");
                    var loadoutItem = loadouts.FirstOrDefault(x => x.Id == lo.Id);
                    if (loadoutItem == null) continue;

                    var oldProductId = loadoutItem.ProductId;
                    var oldPrice = loadoutItem.Price;
                    var oldQuantity = loadoutItem.Quantity;
                    var oldCapacity = loadoutItem.Capacity;

                    if(lo.ProductId.HasValue)
                    {
                        loadoutItem.ProductId = lo.ProductId.Value;
                        loadoutItem.Price = lo.Price;
                        loadoutItem.Quantity = lo.Quantity;
                        loadoutItem.Capacity = lo.Capacity;
                    }
                    else
                    {
                        loadoutItem.ProductId = null;
                        loadoutItem.Quantity = 0;
                        loadoutItem.Price = 0;
                        loadoutItem.Capacity = lo.Capacity;
                    }

                    updatedLoadouts.Add(loadoutItem);
                    if(currentSession != null && !currentSession.IsInprogress)
                    {
                        //Add history
                        var history = new RestockSessionHistory
                        {
                            Id = Guid.NewGuid(),
                            RestockSessionId = currentSession.Id,
                            LoadoutItemId = loadoutItem.Id
                        };
                        var hasChange = false;
                        if(oldProductId != loadoutItem.ProductId)
                        {
                            history.OldProduct = oldProductId;
                            history.NewProduct = loadoutItem.ProductId;
                            hasChange = true;
                        }
                        if(oldPrice != loadoutItem.Price)
                        {
                            history.PriceChange = $"{oldPrice} -> {loadoutItem.Price}";
                            hasChange = true;
                        }
                        if (oldQuantity != loadoutItem.Quantity)
                        {
                            history.QuantityChange = $"{oldQuantity} -> {loadoutItem.Quantity}";
                            hasChange = true;
                        }
                        if (oldCapacity != loadoutItem.Capacity)
                        {
                            history.CapacityChange = $"{oldCapacity} -> {loadoutItem.Capacity}";
                            hasChange = true;
                        }
                        if (hasChange)
                        {
                            newHistories.Add(history);
                        }
                    }
                }
                _detailLogService.Log($"Cloud - UpdateLoadout - update list: {updatedLoadouts.Count}");
                if(updatedLoadouts.Any())
                {
                    using (var uowManager = _iocResolver.ResolveAsDisposable<IUnitOfWorkManager>())
                    {
                        using (var uow = uowManager.Object.Begin(TransactionScopeOption.Suppress))
                        {
                            var dbContext = uowManager.Object.Current.GetDbContext<KonbiCloudDbContext>(MultiTenancySides.Tenant);
                            dbContext.UpdateRange(updatedLoadouts);
                            await dbContext.AddRangeAsync(newHistories);
                            uow.Complete();
                            _detailLogService.Log($"Cloud - UpdateLoadout");
                        }
                    }

                    //Update restock session
                    if(currentSession != null)
                    {
                        currentSession.Total = loadouts.Sum(x => x.Quantity);
                        currentSession.LeftOver = currentSession.Total - currentSession.Sold;
                        await _restockSessionRepository.UpdateAsync(currentSession);
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Cloud - UpdateLoadout error:{ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> UpdateLoadoutItem(LoadoutItemDto loadoutItemInput)
        {
            try
            {
                _detailLogService.Log($"Cloud - UpdateLoadoutItem - input loadout: {JsonConvert.SerializeObject(loadoutItemInput)}");
                var updateItem = await _loadoutItemRepository.FirstOrDefaultAsync(x => x.Id == loadoutItemInput.Id);
                if (updateItem == null) return false;
                var currentSession = await _restockSessionRepository.FirstOrDefaultAsync(x => x.MachineId == loadoutItemInput.MachineId && !x.EndDate.HasValue);
                if(currentSession != null)
                {
                    var history = new RestockSessionHistory
                    {
                        Id = Guid.NewGuid(),
                        RestockSessionId = currentSession.Id,
                        LoadoutItemId = loadoutItemInput.Id
                    };
                    var hasChange = false;
                    if (updateItem.ProductId != loadoutItemInput.ProductId)
                    {
                        history.OldProduct = updateItem.ProductId;
                        history.NewProduct = loadoutItemInput.ProductId;
                        hasChange = true;
                    }
                    if (updateItem.Price != loadoutItemInput.Price)
                    {
                        history.PriceChange = $"{updateItem.Price} -> {loadoutItemInput.Price}";
                        hasChange = true;
                    }
                    if (updateItem.Quantity != loadoutItemInput.Quantity)
                    {
                        history.QuantityChange = $"{updateItem.Quantity} -> {loadoutItemInput.Quantity}";
                        currentSession.Total += loadoutItemInput.Quantity - updateItem.Quantity;
                        currentSession.LeftOver = currentSession.Total - currentSession.Sold;
                        await _restockSessionRepository.UpdateAsync(currentSession);

                        hasChange = true;
                    }
                    if (updateItem.Capacity != loadoutItemInput.Capacity)
                    {
                        history.CapacityChange = $"{updateItem.Capacity} -> {loadoutItemInput.Capacity}";
                        hasChange = true;
                    }
                    if (hasChange)
                    {
                        await _restockSessionHistoryRepository.InsertAsync(history);
                    }
                }
                updateItem.ProductId = loadoutItemInput.ProductId;
                updateItem.Price = loadoutItemInput.Price;
                updateItem.Quantity = loadoutItemInput.Quantity;
                updateItem.Capacity = loadoutItemInput.Capacity;

                await _loadoutItemRepository.UpdateAsync(updateItem);
                _detailLogService.Log($"Cloud - UpdateLoadoutItem - done");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Cloud - UpdateLoadoutItem error:{ex.Message}", ex);
                return false;
            }
        }
    }
}
