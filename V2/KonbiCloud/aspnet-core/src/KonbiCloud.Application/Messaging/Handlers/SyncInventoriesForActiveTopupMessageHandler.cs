using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Inventories;
using KonbiCloud.Machines;
using KonbiCloud.Products;
using Konbini.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging.Handlers
{
    public class SyncInventoriesForActiveTopupMessageHandler : ISyncInventoriesForActiveTopupMessageHandler, ITransientDependency
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IRepository<Topup, Guid> _topupRepository;
        private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
        private readonly IRepository<ProductTag, Guid> _productTagRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly ILogger _logger;

        public SyncInventoriesForActiveTopupMessageHandler(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<Machine,Guid> machineRepository,
            IRepository<Topup, Guid> topupRepository,
            IRepository<InventoryItem, Guid> inventoryRepository,
            IRepository<ProductTag, Guid> productTagRepository,
            IRepository<Product, Guid> productRepository,
            ILogger logger
        )
        {
            _unitOfWorkManager = unitOfWorkManager;
            _machineRepository = machineRepository;
            _topupRepository = topupRepository;
            _inventoryRepository = inventoryRepository;
            _productTagRepository = productTagRepository;
            _productRepository = productRepository;
            _logger = logger;
        }
        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                Guid topUpId = Guid.Empty;
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                    {
                        var machineId = keyValueMessage.MachineId;
                        int? tenantId = null;

                        if (machineId == null)
                        {
                            _logger.Error("Machine id is null");
                            return false;
                        }

                        var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineId);
                        if (machine == null)
                        {
                            _logger.Error("Can not find machine with id = " + machineId);
                            return false;
                        }

                        tenantId = machine.TenantId;

                        var inventories = JsonConvert.DeserializeObject<List<InventoryItem>>(keyValueMessage.JsonValue);
                        if (inventories == null || !inventories.Any())
                        {
                            _logger.Error("No inventories were sent");
                            return false;
                        }

                        if(inventories[0].TopupId == null)
                        {
                            _logger.Error("These inventories do not have topup id");
                            return false;
                        }
                        else
                        {
                            topUpId = inventories[0].TopupId ?? Guid.Empty;
                        }

                        var topUp = await _topupRepository.FirstOrDefaultAsync(x => x.Id == topUpId);
                        if(topUp == null)
                        {
                            _logger.Error("Can not find topUp with id = " + topUpId);
                            return false;
                        }

                        var topUpInventories = _inventoryRepository.GetAll().Where(x => x.TopupId == topUpId);

                        foreach(var item in inventories)
                        {
                            if(!topUpInventories.Any(x => x.Id == item.Id))
                            {
                                if (await _productRepository.CountAsync(x => x.Id == item.ProductId) <= 0)
                                {
                                    _logger.Error("Inventory : " + item.Id + "can not insert because there are no product with id = " + item.ProductId);
                                    continue;
                                }
                                

                                //Insert inventories that not existed in cloud
                                item.MachineId = machineId;
                                item.TenantId = tenantId;

                                //Update producttag
                                var pt = await _productTagRepository.FirstOrDefaultAsync(x => x.Name.Equals(item.TagId));
                                pt.State = Enums.ProductTagStateEnum.Stocked;
                                await _productTagRepository.UpdateAsync(pt);
                            }
                        }

                        await unitOfWork.CompleteAsync();
                    }
                }

                //Update total
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                    {
                        var nt = await _topupRepository.FirstOrDefaultAsync(x => x.Id == topUpId);
                        if (nt != null)
                        {
                            var totalItem = await _inventoryRepository.CountAsync(x => x.DetailTransactionId == null && x.TopupId == topUpId);
                            nt.Total = totalItem;
                            await unitOfWork.CompleteAsync();
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Error when sync inventories to cloud", ex);
                return false;
            }
        } 
    }
}
