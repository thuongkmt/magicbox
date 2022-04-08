using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Repositories;
using KonbiCloud.Configuration;
using KonbiCloud.Enums;
using KonbiCloud.Inventories;
using KonbiCloud.Inventories.Dtos;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Products;
using KonbiCloud.Transactions;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Microsoft.EntityFrameworkCore;

namespace KonbiCloud.Common
{
    using KonbiCloud.Transactions.Dtos;

    public class TestsAppService : KonbiCloudAppServiceBase
    {
        private readonly ISendMessageToCloudService _sendMessageToCloudService;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<Topup, Guid> _topupRepository;
        private readonly IRepository<Inventory, Guid> _inventoryRepository;
        private readonly IRepository<DetailTransaction, long> _transactionReponsitory;
        private readonly ITransactionAppService _transactionService;
        private readonly IInventoriesAppService _inventoriesAppService;

        private Guid machineId;
        public TestsAppService(ITransactionAppService transactionService, IRepository<DetailTransaction, long> transactionReponsitory, ISendMessageToCloudService sendMessageToCloudService, IRepository<Product, Guid> productRepository, IRepository<Topup, Guid> topupRepository, IRepository<Inventory, Guid> inventoryRepository
        , ISettingManager settingManager, IInventoriesAppService inventoriesAppService)
        {
            _sendMessageToCloudService = sendMessageToCloudService;
            _productRepository = productRepository;
            _topupRepository = topupRepository;
            _inventoryRepository = inventoryRepository;
            _inventoriesAppService = inventoriesAppService;
            machineId = Guid.Parse(settingManager.GetSettingValue(AppSettingNames.MachineId));
            _transactionReponsitory = transactionReponsitory;
            _transactionService = transactionService;
        }

        public void SendTestRabbitMq()
        {
            var sampleKV = new KeyValueMessage
            {
                Key = MessageKeys.TestKey,
                MachineId = Guid.NewGuid(),
                Value = "test message"
            };
            _sendMessageToCloudService.SendQueuedMsgToCloud(sampleKV);
        }

        public void TestAddTransctionToCloud()
        {
            var tran = new DetailTransaction
            {
                Amount = (decimal)1.3,
                TranCode = Guid.NewGuid(),
                StartTime = DateTime.Now,
                PaymentTime = DateTime.Now,
                PaymentState = PaymentState.Success,
                PaymentType = PaymentType.IucApi,
                Status = TransactionStatus.Success

            };

            var product = _productRepository.GetAll().FirstOrDefault();

            tran.Products = new List<ProductTransaction>
            {
                new ProductTransaction()
                {
                    Amount = (decimal)1.3,
                    Product =product
                }
            };

            var sampleKV = new KeyValueMessage
            {
                Key = MessageKeys.Transaction,
                MachineId = Guid.Parse("75d38ad8-71fb-9b17-9743-81c4f4958d31"),
                Value = tran
            };
            _sendMessageToCloudService.SendQueuedMsgToCloud(sampleKV);
        }

        public async Task TestAddInventory()
        {
            var product = _productRepository.GetAll().FirstOrDefault();
            var inventory = new Inventory
            {
                TagId = "123",
                Price = 1,
                Product = product
            };

            var currentTopup = await this._topupRepository.GetAll().OrderByDescending(x => x.CreationTime)
           .FirstOrDefaultAsync();
            if (currentTopup == null)
            {
                currentTopup = new Topup { StartDate = DateTime.Now };
                await this._topupRepository.InsertAsync(currentTopup);
            }
            inventory.Topup = currentTopup;

            await _inventoryRepository.InsertAsync(inventory);
            await CurrentUnitOfWork.SaveChangesAsync();


            if (_sendMessageToCloudService.SendQueuedMsgToCloud(new KeyValueMessage()
            {
                MachineId = machineId,
                Key = MessageKeys.Inventory,
                Value = inventory
            }))
            {
                inventory = inventory.MarkSync();
                await _inventoryRepository.UpdateAsync(inventory);
            }



        }

        /// <summary>
        /// DU MA
        /// </summary>
        /// <returns></returns>
        public async Task TestAddTransaction()
        {
            var product = await _productRepository.GetAll().FirstOrDefaultAsync();
            var inventory = await _inventoryRepository.GetAll().FirstOrDefaultAsync();

            var list = new List<FridgeTransactionInventoryDto>();
            var data = new FridgeTransactionInventoryDto();
            data.Id = inventory.Id;
            data.Product = new FridgeTransactionProductDto();
            list.Add(data);

            await _transactionService.AddTransaction(list);
        }

        public async Task TestProcessTopup()
        {
            var topup = await _inventoriesAppService.GetCurrentTopup();
            if (topup.IsProcessing) await _inventoriesAppService.EndTopup(5000);

            await _inventoriesAppService.NewTopup();
            var product = await _productRepository.GetAll().FirstAsync();
            var secondProduct = await _productRepository.GetAll().Skip(1).FirstAsync();
            var newInventory = new CreateOrEditInventoryDto
            {
                Price = 10,
                TagId = new Random(100100).ToString(),
                ProductId = product.Id,
            };
            var newInventory1 = new CreateOrEditInventoryDto
            {
                Price = 10,
                TagId = new Random(100).ToString(),
                ProductId = secondProduct.Id,
            };
            await _inventoriesAppService.CreateOrEdit(newInventory);
            await _inventoriesAppService.CreateOrEdit(newInventory1);

            await _inventoriesAppService.EndTopup(2);
        }
    }
}
