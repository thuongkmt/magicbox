
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Inventories;
using KonbiCloud.Machines;
using KonbiCloud.Products;
using KonbiCloud.Transactions;
using Konbini.Messages;
using Newtonsoft.Json;

namespace KonbiCloud.Messaging.Handlers
{
    public class UpdateInventoryListMessageHandler:InventoryHandlerBase,IUpdateInventoryListMessageHandler  , ITransientDependency
    {
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<DetailTransaction, long> _tranRepository;

        private readonly ILogger _logger;
        public UpdateInventoryListMessageHandler(IRepository<InventoryItem, Guid> inventoryRepository, IRepository<Product, Guid> productRepository, IRepository<Topup, Guid> topupRepository, IRepository<Machine, Guid> machineRepository, IUnitOfWorkManager unitOfWorkManager, IRepository<DetailTransaction, long> tranRepository) : base(inventoryRepository, productRepository, topupRepository)
        {
            _machineRepository = machineRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _tranRepository = tranRepository;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    var inventories = JsonConvert.DeserializeObject<List<InventoryItem>>(keyValueMessage.JsonValue);
                    var currentTopup = inventories[0].Topup;

                    currentTopup = await ProcessInventoryTopup(currentTopup, keyValueMessage);

                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == keyValueMessage.MachineId);
                    if (machine == null) return true;



                    foreach (var inventory in inventories)
                    {
                        inventory.TenantId = machine.TenantId;
                        await ProcessSingleInventory(keyValueMessage.MachineId, inventory, currentTopup, true);
                    }


                    await unitOfWork.CompleteAsync();
                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error("ProcessInventory", e);
                return false;
            }
        }
    }
}
