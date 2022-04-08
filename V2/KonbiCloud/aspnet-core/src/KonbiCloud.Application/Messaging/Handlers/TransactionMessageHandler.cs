using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Castle.Core.Logging;
using KonbiCloud.Inventories;
using KonbiCloud.Machines;
using KonbiCloud.Products;
using KonbiCloud.SignalR;
using KonbiCloud.Transactions;
using Konbini.Messages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KonbiCloud.Messaging.Handlers
{
    public class TransactionMessageHandler:ITransactionMessageHandler, ITransientDependency
    {

        private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
        private readonly IRepository<Topup, Guid> _topupRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<DetailTransaction, long> _tranRepository;
        private readonly IMagicBoxMessageCommunicator _magicBoxMessage;
        private readonly IRepository<ProductTag, Guid> _productTagRepository;
        private readonly IRepository<CashlessDetail, int> _cashlessDetailRepository;
        private readonly IRepository<TransactionDetail, Guid> _transactionDetailRepository;

        private readonly ILogger _logger;

        public TransactionMessageHandler(IRepository<InventoryItem, Guid> inventoryRepository, 
            IRepository<Topup, Guid> topupRepository, 
            IRepository<Machine, Guid> machineRepository, 
            IUnitOfWorkManager unitOfWorkManager, 
            IRepository<Product, Guid> productRepository, 
            IRepository<DetailTransaction, long> tranRepository,
            ILogger logger,
            IMagicBoxMessageCommunicator magicBoxMessage,
            IRepository<ProductTag, Guid> productTagRepository,
            IRepository<CashlessDetail, int> cashlessDetailRepository,
            IRepository<TransactionDetail, Guid> transactionDetailRepository)
        {
            _inventoryRepository = inventoryRepository;
            _topupRepository = topupRepository;
            _machineRepository = machineRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _productRepository = productRepository;
            _tranRepository = tranRepository;
            _logger = logger;
            _magicBoxMessage = magicBoxMessage;
            _productTagRepository = productTagRepository;
            _cashlessDetailRepository = cashlessDetailRepository;
            _transactionDetailRepository = transactionDetailRepository;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant,AbpDataFilters.MustHaveTenant))
                    {
                        _logger.Info("Start sync transaction");

                        int? tenantId = null;
                        var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == keyValueMessage.MachineId);

                        if (machine != null)
                        {
                            tenantId = machine.TenantId;
                        }
                        var tran = JsonConvert.DeserializeObject<DetailTransaction>(keyValueMessage.JsonValue);
                        tran.LocalTranId = tran.Id;
                        tran.MachineId = keyValueMessage.MachineId;
                        tran.TenantId = tenantId;

                        var inventoryIds = tran.Inventories.Select(x => x.Id).ToList();
                        var inventories = tran.Inventories;

                        //calculate Sold for Topup
                        var topup = await _topupRepository.GetAll().Where(x => x.EndDate == null && x.MachineId == keyValueMessage.MachineId)
                            .OrderByDescending(x => x.StartDate).FirstOrDefaultAsync();

                        _logger.Info($"Inventory {JsonConvert.SerializeObject(tran.Inventories)}");
                        //topup.Sold += tran.Inventories.Count;
                        //await _topupRepository.UpdateAsync(topup);
                        tran.Topup = topup;


                        //update inventory
                        var existedInventories = await _inventoryRepository.GetAll().Where(x => inventoryIds.Contains(x.Id)).ToListAsync();

                        var newInventories = new Collection<InventoryItem>();
                        foreach (var inventory in inventories)
                        {
                            var existedInventory = existedInventories.SingleOrDefault(x => x.Id == inventory.Id);
                            var product = await _productRepository.FirstOrDefaultAsync(x => x.Id == inventory.ProductId);
                            if (product == null) continue;

                            if (existedInventory == null)
                            {
                                continue;
                                //inventory.DetailTransactionId = tran.Id;
                                //if (product != null) inventory.Product = product;
                                //await _inventoryRepository.InsertOrUpdateAsync(inventory);
                                //newInventories.Add(inventory);
                            }
                            else
                            {
                                existedInventory.DetailTransactionId = tran.Id;
                                existedInventory.Product = product;
                                await _inventoryRepository.UpdateAsync(existedInventory);

                                var productTag = await _productTagRepository.FirstOrDefaultAsync(x => x.Name.Equals(inventory.TagId) && x.ProductId == inventory.ProductId);
                                if (productTag != null)
                                {
                                    productTag.State = Enums.ProductTagStateEnum.Sold;
                                    await _productTagRepository.UpdateAsync(productTag);
                                }

                                newInventories.Add(existedInventory);
                            }
                        }

                        /// ////////////////////////////////////
                        /// Start
                        /// Description: uppate data into [TransactionDetail] table
                        /// ////////////////////////////////////
                        var newTransactionDetail = new Collection<TransactionDetail>();
                        foreach (var inventory in inventories)
                        {
                            TransactionDetail transDetail = new TransactionDetail
                            {
                                TenantId = tenantId,
                                TagId = inventory.TagId,
                                Price = inventory.Price,
                                TopupId = inventory.TopupId.ToString(),
                                MachineId = inventory.MachineId.ToString(),
                                LocalInventoryId = inventory.Id.ToString(),
                                TransactionId = tran.Id,
                                ProductId = inventory.ProductId
                            };
                            newTransactionDetail.Add(transDetail);
                        }
                        _logger.Info($"TransactionDetail {JsonConvert.SerializeObject(newTransactionDetail)}");
                        /// ////////////////////////////////////
                        /// End
                        /// Description: uppate data into [TransactionDetail] table 
                        /// ////////////////////////////////////


                        tran.Id = 0;
                        tran.Inventories = newInventories;
                        tran.TransactionDetails = newTransactionDetail;

                        if (tran.Status == Enums.TransactionStatus.Success)
                        {
                            if(topup != null)
                            {
                                topup.Sold += newInventories.Count;
                            }
                        }

                        //Insert/Update cashless
                        if (tran.CashlessDetail != null)
                        {
                            var cashless = tran.CashlessDetail;
                            var newCashless = new CashlessDetail
                            {
                                Amount = cashless.Amount,
                                Tid = cashless.Tid,
                                Mid = cashless.Mid,
                                Invoice = cashless.Invoice,
                                Batch = cashless.Batch,
                                CardLabel = cashless.CardLabel,
                                CardNumber = cashless.CardNumber,
                                Rrn = cashless.Rrn,
                                ApproveCode = cashless.ApproveCode,
                                EntryMode = cashless.EntryMode,
                                AppLabel = cashless.AppLabel,
                                Aid = cashless.Aid,
                                Tc = cashless.Tc,
                                TenantId = tenantId
                            };

                            tran.CashlessDetail = newCashless;
                        }
                        //save transction
                        await _tranRepository.InsertAsync(tran);
                        if (topup != null)
                        {
                            await _topupRepository.UpdateAsync(topup);
                        }
                        
                        await unitOfWork.CompleteAsync();


                        //Notify Client
                        await _magicBoxMessage.SendMessageToAllClient(
                                                   new MagicBoxMessage
                                                   {
                                                       MessageType = MagicBoxMessageType.Transaction,
                                                   });

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Transaction Message Handler", e);
                return false;
            }

        }
    }
}
