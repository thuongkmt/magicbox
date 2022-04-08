using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Configuration;
using KonbiCloud.Enums;
using KonbiCloud.Inventories;
using Konbini.Messages;
using Konbini.Messages.Services;
using Newtonsoft.Json;

namespace KonbiCloud.Messaging
{
    public class ProductTagsMessageHandler : IProductTagsMessageHandler
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
        private readonly IRepository<Products.Product, Guid> _productRepository;
        private readonly ILogger _logger;
        private readonly ISendMessageToCloudService _sendMessageToCloudService;
        private readonly bool _useCloud;

        public ProductTagsMessageHandler(IUnitOfWorkManager unitOfWorkManager,
            IRepository<InventoryItem, Guid> inventoryRepository,
            ILogger logger,
            ISendMessageToCloudService sendMessageToCloudService,
            IRepository<Products.Product,Guid> productRepository,
            ISettingManager settingManager)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _inventoryRepository = inventoryRepository;
            _logger = logger;
            _productRepository = productRepository;
            _sendMessageToCloudService = sendMessageToCloudService;
            bool.TryParse(settingManager.GetSettingValue(AppSettingNames.UseCloud), out _useCloud);
        }
        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    var inventories = JsonConvert.DeserializeObject<List<InventoryItem>>(keyValueMessage.JsonValue);
                    var products = inventories.Select(x => x.Product).ToList();
                    var productids = products.Select(x => x.Id).ToList();
                    var productsInDb = _productRepository.GetAll().Where(x => productids.Contains(x.Id)).ToList();
                    foreach (var item in inventories)
                    {
                        if(!productsInDb.Any(x=>x.Id==item.Product.Id))
                        {
                            item.Product = await _productRepository.InsertAsync(item.Product);
                        }
                        item.State = Enums.TagState.Active;
                        await _inventoryRepository.InsertAsync(item);
                    }
                    await unitOfWork.CompleteAsync();
                    Task.Factory.StartNew(() => ChangeTagState(keyValueMessage.MachineId, inventories));  //send data to cloud, mark as synced

                    return true;
                }
            }
            catch (Exception e)
            {
                _logger.Error("Process Product Tags Message Handler", e);
                return false;
            }
        }

        private bool ChangeTagState(Guid machineId,IList<InventoryItem> items)
        {
            var kv = new KeyValueMessage
            {
                MachineId = machineId,
                Key = Konbini.Messages.Enums.MessageKeys.UpdateTagState,
                Value = new
                {
                    Ids = items.Select(x => x.Id).ToList(),
                    State = TagState.Active
                }
            };
            // TrungPQ: Check use Cloud.
            if (_useCloud)
            {
                _sendMessageToCloudService.SendQueuedMsgToCloud(kv);
            }
            return true;
        }
    }
}
