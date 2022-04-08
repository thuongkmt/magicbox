using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Dto;
using KonbiCloud.Inventories;
using Konbini.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging.Handlers
{
    public class ChangeTagStateMessageHandler : IChangeTagStateMessageHandler, ITransientDependency
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<InventoryItem, Guid> _inventoryItemRepository;
        private readonly ILogger _logger;

        public ChangeTagStateMessageHandler(IUnitOfWorkManager unitOfWorkManager,
           IRepository<InventoryItem, Guid> inventoryRepository,
           ILogger logger)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _inventoryItemRepository = inventoryRepository;           
            _logger = logger;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                return true;
                //using (var unitOfWork = _unitOfWorkManager.Begin())
                //{

                //    var data = JsonConvert.DeserializeObject<ChangeTagStateDto>(keyValueMessage.JsonValue);
                //    foreach (var item in data.Ids)
                //    {
                //        _inventoryItemRepository.Update(item, x => x.State = data.State);
                //    }
                //    await unitOfWork.CompleteAsync();


                //    return true;
                //}
            }
            catch (Exception e)
            {
                _logger.Error("Process Change Tag State Message Handler", e);
                return false;
            }
        }
    }
}
