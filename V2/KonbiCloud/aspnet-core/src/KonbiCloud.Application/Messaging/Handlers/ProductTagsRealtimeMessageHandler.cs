using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Timing;
using Castle.Core.Logging;
using KonbiCloud.BackgroundJobs;
using KonbiCloud.Dto;
using KonbiCloud.Enums;
using KonbiCloud.Inventories;
using KonbiCloud.Inventories.Dtos;
using KonbiCloud.Machines;
using KonbiCloud.Products;
using KonbiCloud.SignalR;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KonbiCloud.Messaging.Handlers
{
    public class ProductTagsRealtimeMessageHandler : IProductTagsRealtimeMessageHandler, ITransientDependency
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly ISendMessageToMachineClientService _sendMessageToMachineClientService;
        private readonly ICacheManager _cacheManager;
        private readonly IMagicBoxMessageCommunicator _magicBoxMessage;
        private readonly UpdateMachineInventoryToDbJob _updateMachineInventoryToDbJob;

        public ProductTagsRealtimeMessageHandler( IUnitOfWorkManager unitOfWorkManager, 
            IRepository<Machine, Guid> machineRepository,  
            IRepository<InventoryItem, Guid> inventoryRepository, 
            ILogger logger,
            ISendMessageToMachineClientService sendMessageToMachineClientService,
            IRepository<Product,Guid> productRepository,
            ICacheManager cacheManager,
            UpdateMachineInventoryToDbJob updateMachineInventoryToDbJob,
            IMagicBoxMessageCommunicator magicBoxMessage) 
        {
            _cacheManager = cacheManager;
            _unitOfWorkManager = unitOfWorkManager;
            _machineRepository = machineRepository;
            _logger = logger;
            _inventoryRepository = inventoryRepository;
            _sendMessageToMachineClientService = sendMessageToMachineClientService;
            _productRepository = productRepository;
            _sendMessageToMachineClientService.ErrorAction = (e, msg) => _logger.Error(msg, e);
            _magicBoxMessage = magicBoxMessage;
            _updateMachineInventoryToDbJob = updateMachineInventoryToDbJob;
        }

     
        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                var productMaps = JsonConvert.DeserializeObject<List<KeyValuePair<string,string>>>(keyValueMessage.JsonValue);
                var cacheItem = new MachineInventoryCacheDto() {
                    LastUpdated = Clock.Now,
                    MachineId = keyValueMessage.MachineId,
                    Tags = new List<TagProductDto>()

                };
                productMaps.ForEach(el => cacheItem.Tags.Add(new TagProductDto() { Tag = el.Key, ProductName = el.Value }));
                //Set cache
                await _cacheManager.GetCache(Common.Const.ProductTagRealtime)
                                   .SetAsync(keyValueMessage.MachineId.ToString(), cacheItem);

                // register the machine being update its inventory into db via a background job
                _updateMachineInventoryToDbJob.RequestUpdatingInventory(keyValueMessage.MachineId);

               
                //Notify Client
                await _magicBoxMessage.SendMessageToAllClient(
                                           new MagicBoxMessage
                                           {
                                               MessageType = MagicBoxMessageType.CurrentInventory
                                           });

                return true;

            }
            catch (Exception e)
            {
                _logger.Error("Process Product Tags Realtime Message Handler", e);
                return false;
            }
        }


        private bool SendDataToMachine(Guid machineId, IList<InventoryItem> items)
        {
            try
            {
                var kv = new KeyValueMessage()
                {
                    MachineId = machineId,
                    Key = MessageKeys.ProductRfidTags,
                    Value = items
                };
                _sendMessageToMachineClientService.SendQueuedMsgToMachines(kv, CloudToMachineType.ToMachineId);
                return true;
            }
            catch (Exception e)
            {
                _logger.Error("SendDataToMachine",e);
                return false;
            }
           
        }
    }
}
