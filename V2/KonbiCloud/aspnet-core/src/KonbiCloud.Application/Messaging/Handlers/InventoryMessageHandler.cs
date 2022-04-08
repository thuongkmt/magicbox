using System;
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
    public class InventoryMessageHandler:InventoryHandlerBase,IInventoryMessageHandler    , ITransientDependency
    {
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<DetailTransaction, long> _tranRepository;

        private readonly ILogger _logger;

        public InventoryMessageHandler(IRepository<Topup, Guid> topupRepository, IUnitOfWorkManager unitOfWorkManager, IRepository<Product, Guid> productRepository, IRepository<Machine, Guid> machineRepository, IRepository<DetailTransaction, long> tranRepository, IRepository<InventoryItem, Guid> inventoryRepository, ILogger logger):base(inventoryRepository,productRepository,topupRepository)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _machineRepository = machineRepository;
            _tranRepository = tranRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    var inventory = JsonConvert.DeserializeObject<InventoryItem>(keyValueMessage.JsonValue);

                    var currentTopup = inventory.Topup;

                    currentTopup = await ProcessInventoryTopup(currentTopup, keyValueMessage);

                    var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == keyValueMessage.MachineId);
                    if (machine == null) return true;
                    inventory.TenantId = machine.TenantId;

                    var result = await ProcessSingleInventory(keyValueMessage.MachineId, inventory, currentTopup);
                    if(!result)
                    {
                        return false;
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
