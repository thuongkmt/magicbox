using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Configuration;
using KonbiCloud.Inventories;
using KonbiCloud.Products;
using KonbiCloud.Products.Dtos;
using Konbini.Messages;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging.Handlers
{
    class ProductMachinePriceMessageHandler : IProductMachinePriceMessageHandler
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
        private readonly ILogger _logger;
        private readonly string _machineId;
        private string BASE_URL { get; set; }

        /// <summary>
        /// Inject ProductMachinePriceMessageHandler.
        /// </summary>
        /// <param name="unitOfWorkManager"></param>
        /// <param name="productCategoryRepository"></param>
        /// <param name="productCategoryRelationRepository"></param>
        /// <param name="productRepository"></param>
        public ProductMachinePriceMessageHandler(
            ISettingManager settingManager,
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<Product, Guid> productRepository,
            IRepository<InventoryItem, Guid> inventoryRepository)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
            _machineId = settingManager.GetSettingValue(AppSettingNames.MachineId);
            BASE_URL = "http://localhost:9000";
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                var client = new HttpClient { BaseAddress = new Uri(BASE_URL) };
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    var productMachinePrice = JsonConvert.DeserializeObject<ProductMachinePrice>(keyValueMessage.JsonValue);
                    if (productMachinePrice != null)
                    {
                        // Check machine.
                        if (_machineId == productMachinePrice.MachineId.ToString())
                        {
                            var product = _productRepository.FirstOrDefault(productMachinePrice.ProductId);

                            if (product != null)
                            {
                                product.Price = (double)productMachinePrice.Price;
                            }

                            var inventories = _inventoryRepository.GetAll().Where(x => x.ProductId == productMachinePrice.ProductId && x.DetailTransactionId == null);

                            foreach(var item in inventories)
                            {
                                item.Price = (double)productMachinePrice.Price;
                            }
                        }

                    }

                    await unitOfWork.CompleteAsync();

                    _logger?.Info($"Auto sync ProductMachinePrice: { productMachinePrice }");

                    await client.PostAsync("/api/machine/inventory/reload", null);

                    _logger?.Info($"Reload inventory for changing machine product price");

                    return true;
                }
            }
            catch (Exception e)
            {
                _logger?.Error("Process product machine price", e);
                return false;
            }
        }
    }
}
