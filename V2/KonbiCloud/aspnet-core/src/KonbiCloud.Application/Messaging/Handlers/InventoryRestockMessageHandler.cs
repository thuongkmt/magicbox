using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Timing;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Inventories;
using KonbiCloud.Machines;
using KonbiCloud.Products;
using Konbini.Messages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging
{
    public class InventoryRestockMessageHandler : IInventoryRestockMessageHandler, ITransientDependency
    {
        private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
        private readonly IRepository<Topup, Guid> _topupRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger _logger;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<ProductTag, Guid> _productTagRepository;
        private readonly IRepository<TopupHistory, long> _topupHistoryRepository;
        private readonly IDetailLogService _detailLogService;

        public InventoryRestockMessageHandler(IRepository<InventoryItem, Guid> inventoryRepository,
                                              IRepository<Topup, Guid> topupRepository,
                                              IRepository<Machine, Guid> machineRepository,
                                              IUnitOfWorkManager unitOfWorkManager, ILogger logger,
                                              IRepository<Product, Guid> productRepository,
                                              IRepository<ProductTag, Guid> productTagRepository,
                                              IRepository<TopupHistory, long> topupHistoryRepository,
                                              IDetailLogService detailLogService)
        {
            _inventoryRepository = inventoryRepository;
            _topupRepository = topupRepository;
            _machineRepository = machineRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
            _productRepository = productRepository;
            _productTagRepository = productTagRepository;
            _detailLogService = detailLogService;
            _topupHistoryRepository = topupHistoryRepository;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                Guid newTopupId = Guid.Empty;
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                    {
                        int? tenantId = null;
                        //Check machine exist
                        var mc = await _machineRepository.FirstOrDefaultAsync(x => x.Id == keyValueMessage.MachineId);
                        if (mc == null)
                        {
                            _detailLogService.Log("Can not find the machine with id = " + keyValueMessage.MachineId);
                            return false;
                        } 
                        tenantId = mc.TenantId;
                        _detailLogService.Log("Restock from machine : " + keyValueMessage.MachineId + " and tenant: " + tenantId);

                        var newInventories = JsonConvert.DeserializeObject<List<InventoryItem>>(keyValueMessage.JsonValue);
                        if (newInventories == null || !newInventories.Any()) return true;

                        //Close all topups
                        var unclosedTopups = await _topupRepository.GetAll().Where(x =>
                                x.MachineId == keyValueMessage.MachineId && x.EndDate == null).OrderByDescending(x => x.CreationTime).ToListAsync();
                        foreach (var unclosedTopup in unclosedTopups)
                        {
                            unclosedTopup.EndDate = Clock.Now;
                        }

                        newInventories[0].Topup.MachineId = keyValueMessage.MachineId;
                        newInventories[0].Topup.Total = 0;
                        newInventories[0].Topup.TenantId = tenantId;

                        var newTopup = await _topupRepository.InsertAsync(newInventories[0].Topup);

                        _detailLogService.Log("New topup: " + newInventories[0].Topup);

                        newTopupId = newTopup.Id;

                        //Move unsold inventories to new topup
                        var oldTopupId = unclosedTopups.Any() ? unclosedTopups[0].Id : Guid.Empty;
                        var oldInventories = await _inventoryRepository.GetAllListAsync(x => x.DetailTransactionId == null && x.TopupId == oldTopupId);
                        foreach (var oldInventory in oldInventories)
                        {
                            if (newInventories.Any(x => x.TagId.Equals(oldInventory.TagId)))
                            {
                                oldInventory.State = Enums.TagState.Removed;
                            }
                            else
                            {
                                oldInventory.State = Enums.TagState.Stocked;
                                oldInventory.Topup = newTopup;
                                
                                //Insert old product into TopupInventory
                                await _topupHistoryRepository.InsertAsync(new TopupHistory
                                {
                                    TopupId = newTopupId,
                                    InventoryId = oldInventory.Id,
                                    TenantId = tenantId
                                });
                            }
                        }

                        //Insert new inventories
                        var existInventories = await _inventoryRepository.GetAllListAsync(x => x.MachineId == mc.Id && x.TopupId == newTopup.Id);
                        foreach (var inventory in newInventories)
                        {
                            //Check to skip is old invetory item
                            if (oldInventories.Any(x => x.TagId.Equals(inventory.TagId))) continue;

                            if (await _productRepository.CountAsync(x => x.Id == inventory.ProductId) <= 0) continue;
                            if (existInventories.Any(x => x.Id == inventory.Id)) continue;

                            //Insert new product into TopupInventory
                            await _topupHistoryRepository.InsertAsync(new TopupHistory
                            {
                                TopupId = newTopupId,
                                InventoryId = inventory.Id,
                                TenantId = tenantId
                            });

                            inventory.MachineId = mc.Id;
                            inventory.TenantId = tenantId;
                            await _inventoryRepository.InsertAsync(inventory);

                            //Update producttag
                            var pt = await _productTagRepository.FirstOrDefaultAsync(x => x.Name.Equals(inventory.TagId));
                            pt.State = Enums.ProductTagStateEnum.Stocked;
                            await _productTagRepository.UpdateAsync(pt);
                        }
                        await unitOfWork.CompleteAsync();
                    }
                }

                //Update total
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                    {
                        var nt = await _topupRepository.FirstOrDefaultAsync(x => x.Id == newTopupId);
                        if (nt != null)
                        {
                            var totalItem = await _inventoryRepository.CountAsync(x => x.DetailTransactionId == null && x.TopupId == newTopupId);
                            nt.Total = totalItem;
                            await unitOfWork.CompleteAsync();
                        }
                    }
                }
                
                return true;
            }
            catch (Exception e)
            {
                _logger.Error("Process Restock", e);
                return false;
            }
        }
    }
}
