using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Inventories;
using KonbiCloud.Machines;
using KonbiCloud.SignalR;
using Konbini.Messages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KonbiCloud.Messaging
{
    public class TopupMessageHandler : ITopupMessageHandler, ITransientDependency
    {

        private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
        private readonly IRepository<Topup, Guid> _topupRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger _logger;
        private readonly IMagicBoxMessageCommunicator _magicBoxMessage;

        public TopupMessageHandler(IRepository<InventoryItem, Guid> inventoryRepository,
            IRepository<Topup, Guid> topupRepository, IRepository<Machine, Guid> machineRepository,
            IUnitOfWorkManager unitOfWorkManager, ILogger logger,IMagicBoxMessageCommunicator magicBoxMessage)
        {
            _inventoryRepository = inventoryRepository;
            _topupRepository = topupRepository;
            _machineRepository = machineRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
            _magicBoxMessage = magicBoxMessage;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    var topup = JsonConvert.DeserializeObject<Topup>(keyValueMessage.JsonValue);
                    var existedTopup = await _topupRepository.FirstOrDefaultAsync(x => x.Id == topup.Id);
                    var machine = await _machineRepository.GetAsync(keyValueMessage.MachineId);
                    topup.Machine = machine;

                    var unclosedTopups = await _topupRepository.GetAll().Where(x =>
                            x.MachineId == keyValueMessage.MachineId && x.EndDate == null)
                        .ToListAsync();

                    foreach (var unclosedTopup in unclosedTopups)
                    {
                        if (unclosedTopup.Id == topup.Id) continue;
                        unclosedTopup.EndDate = topup.StartDate;
                        await _topupRepository.UpdateAsync(unclosedTopup);
                    }

                    var total = await _inventoryRepository.GetAll()
                        .Where(x => x.TopupId == topup.Id && x.MachineId == keyValueMessage.MachineId &&
                                    x.Transaction == null).CountAsync();

                    if (existedTopup != null)
                    {
                        existedTopup.Total = total;
                        await _topupRepository.UpdateAsync(existedTopup);
                    }
                    else
                    {
                        topup.Total = total;
                        if (machine != null)
                            topup.TenantId = machine.TenantId;
                        await _topupRepository.InsertAsync(topup);
                    }

                    await unitOfWork.CompleteAsync();


                    //Notify Client
                    await _magicBoxMessage.SendMessageToAllClient(
                                               new MagicBoxMessage
                                               {
                                                   MessageType = MagicBoxMessageType.Topup,
                                               });

                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error("ProcessTopup", e);
                return false;
            }
        }
    }
}
