using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using KonbiCloud.Authorization;
using KonbiCloud.Common;
using KonbiCloud.Enums;
using KonbiCloud.Inventories;
using KonbiCloud.Machines;
using KonbiCloud.Products;
using KonbiCloud.SignalR;
using KonbiCloud.Transactions.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KonbiCloud.Transactions
{
    [AbpAuthorize(AppPermissions.Pages_Transactions)]
    public class TransactionAppService : KonbiCloudAppServiceBase, ITransactionAppService
    {
        private readonly IRepository<DetailTransaction, long> _transactionRepository;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IRepository<Session, Guid> _sessionRepository;
        private readonly IDetailLogService detailLogService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<InventoryItem, Guid> _inventoryRepository;
        private readonly IRepository<ProductTag, Guid> _productTagRepository;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IRepository<Topup, Guid> _topupRepository;
        private readonly IRepository<DetailTransaction, long> _tranRepository;
        private readonly IMagicBoxMessageCommunicator _magicBoxMessage;

        public TransactionAppService(IRepository<DetailTransaction, long> transactionRepository,
                                     IRepository<Machine, Guid> machineRepository,
                                     IRepository<Session, Guid> sessionRepository,
                                     IDetailLogService detailLog,
                                     IUnitOfWorkManager unitOfWorkManager,
                                     IRepository<InventoryItem, Guid> inventoryRepository,
                                     IRepository<ProductTag, Guid> productTagRepository,
                                     IRepository<Product, Guid> productRepository,
                                     IRepository<Topup, Guid> topupRepository,
                                     IRepository<DetailTransaction, long> tranRepository,
                                     IMagicBoxMessageCommunicator magicBoxMessage
            )
        {
            _transactionRepository = transactionRepository;
            _machineRepository = machineRepository;
            _sessionRepository = sessionRepository;
            _unitOfWorkManager = unitOfWorkManager;
            this.detailLogService = detailLog;
            _inventoryRepository = inventoryRepository;
            _productTagRepository = productTagRepository;
            _productRepository = productRepository;
            _topupRepository = topupRepository;
            _tranRepository = tranRepository;
            _magicBoxMessage = magicBoxMessage;
        }

        [AbpAuthorize(AppPermissions.Pages_Transactions)]

        public async Task<PagedResultDto<TransactionDto>> GetAllTransactions(TransactionInput input)
        {
            try
            {

                var transactions = _transactionRepository.GetAllIncluding()
                    .WhereIf(!string.IsNullOrWhiteSpace(input.CardLabel), e => e.CashlessDetail.CardLabel.ToString().Equals(input.CardLabel))
                    .WhereIf(input.FromDate.HasValue, e => e.PaymentTime.Date >= input.FromDate)
                    .WhereIf(input.ToDate.HasValue, e => e.PaymentTime.Date <= input.ToDate.Value.AddDays(1))
                    .WhereIf(input.MachineFilter.HasValue, e => e.MachineId.Equals(input.MachineFilter));

                if (input.TransactionType == 1)
                {
                    transactions = transactions.Where(e => e.Status == TransactionStatus.Success);
                }
                else
                {
                    transactions = transactions.Where(e => e.Status != TransactionStatus.Success);
                    transactions = transactions.WhereIf(!string.IsNullOrWhiteSpace(input.StateFilter), e => e.Status.ToString().Equals(input.StateFilter));
                }

                transactions = transactions
                   .Include(x => x.Machine)
                   .Include(x => x.Session)
                   .Include(x => x.Inventories)
                   .Include(x => x.TransactionDetails)
                   .Include(x => x.CashlessDetail)
                   .Include("Inventories.Product");

                var totalCount = await transactions.CountAsync();
                var tranLists = await transactions.OrderBy(input.Sorting ?? "PaymentTime desc")
                    .PageBy(input)
                    .ToListAsync();

                var list = new List<TransactionDto>();
                foreach (var x in tranLists)
                {
                    var newTran = new TransactionDto()
                    {
                        Id = x.Id,
                        TranCode = x.TranCode.ToString(),
                        Buyer = x.Buyer,
                        PaymentTime = x.PaymentTime,
                        Amount = x.Amount,
                        States = x.Status.ToString(),
                        Products = x.Inventories.Select(i => new ProductTransactionDto()
                        {
                            Product = i.Product,
                            Amount = (decimal)i.Price,
                            TagId = i.TagId
                        }).ToList(),
                        TransactionDetails = x.TransactionDetails.Select(i => new TransactionDetailDto() { 
                            Price = i.Price,
                            TopupId = i.TopupId,
                            LocalInventoryId = i.LocalInventoryId,
                            ProductName = i.Product.Name,
                            TagId = i.TagId
                        }).ToList(),
                        Machine = x.Machine == null ? null : x.Machine.Name,
                        Session = x.Session == null ? null : x.Session.Name,
                        TransactionId = x.Machine == null ? x.LocalTranId.ToString() : $"{x.Machine.Name}_{x.LocalTranId.ToString()}",
                        PaidAmount = x.CashlessDetail == null ? 0 : x.CashlessDetail.Amount,
                        CardLabel = x.CashlessDetail == null ? null : x.CashlessDetail.CardLabel,
                        CardNumber = x.CashlessDetail == null ? null : x.CashlessDetail.CardNumber
                    };

                    list.Add(newTran);
                }
                detailLogService.Log($"Get All Transactions returns : {list.Count}");
                return new PagedResultDto<TransactionDto>(totalCount, list);
            }
            catch (Exception ex)
            {
                Logger.Error($"Get all Transactions {ex.Message}", ex);
                return new PagedResultDto<TransactionDto>(0, new List<TransactionDto>());
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Transactions)]
        //[AbpAllowAnonymous]
        public async Task<PagedResultDto<TransactionFinanceReportDto>> GetAllTransactionFinanceReport(TransactionInput input)
        {
            try
            {
                var transactions = _transactionRepository.GetAllIncluding()
                    .WhereIf(!string.IsNullOrWhiteSpace(input.CardLabel), e => e.CashlessDetail.CardLabel.Equals(input.CardLabel, StringComparison.OrdinalIgnoreCase))
                    .WhereIf(input.FromDate.HasValue, e => e.PaymentTime.Date >= input.FromDate)
                    .WhereIf(input.ToDate.HasValue, e => e.PaymentTime.Date <= input.ToDate.Value.AddDays(1))
                    .WhereIf(input.MachineFilter.HasValue, e => e.MachineId.Equals(input.MachineFilter));

                // Get all transaction by use -1
                if (input.TransactionType != -1)
                {
                    if (input.TransactionType == 1)
                    {
                        transactions = transactions.Where(e => e.Status == TransactionStatus.Success);
                    }
                    else
                    {
                        transactions = transactions.Where(e => e.Status != TransactionStatus.Success);
                        transactions = transactions.WhereIf(!string.IsNullOrWhiteSpace(input.StateFilter), e => e.Status.ToString().Equals(input.StateFilter));
                    }
                }



                transactions = transactions
                   .Include(x => x.Machine)
                   .Include(x => x.Session)
                   .Include(x => x.Inventories)
                   .Include(x => x.CashlessDetail)
                   .Include("Inventories.Product");

                var totalCount = await transactions.CountAsync();
                var tranLists = await transactions.OrderBy(input.Sorting ?? "PaymentTime desc")
                    .PageBy(input)
                    .ToListAsync();

                var list = new List<TransactionFinanceReportDto>();
                foreach (var x in tranLists)
                {
                    var newTran = new TransactionFinanceReportDto()
                    {
                        Machine = x.Machine == null ? null : x.Machine.Name,
                        TransactionId = x.Machine == null ? x.LocalTranId.ToString() : $"{x.Machine.Name}_{x.LocalTranId.ToString()}",
                        DateTime = x.PaymentTime,
                        Quantity = x.Inventories.Count,
                        PaymentType = x.CashlessDetail.CardLabel,
                        CardId = x.CashlessDetail.CardNumber,
                        AmountPaid = x.Amount,
                        TransactionStatus = x.Status.ToString()
                    };

                    list.Add(newTran);
                }
                detailLogService.Log($"GetAllTransactionFinanceReport returns : {list.Count}");
                return new PagedResultDto<TransactionFinanceReportDto>(totalCount, list);
            }
            catch (Exception ex)
            {
                Logger.Error($"GetAllTransactionFinanceReport {ex.Message}", ex);
                return new PagedResultDto<TransactionFinanceReportDto>(0, new List<TransactionFinanceReportDto>());
            }
        }

        public async Task<PagedResultDto<TransactionItemsReportDto>> GetAllTransactionItemsReport(TransactionInput input)
        {
            try
            {
                var transactions = _transactionRepository.GetAllIncluding()
                    // .WhereIf(!string.IsNullOrWhiteSpace(input.SessionFilter), e => e.SessionId.ToString().Equals(input.SessionFilter))
                    .WhereIf(!string.IsNullOrWhiteSpace(input.CardLabel), e => e.CashlessDetail.CardLabel.ToString().Equals(input.CardLabel))
                    .WhereIf(input.FromDate.HasValue, e => e.PaymentTime.Date >= input.FromDate)
                    .WhereIf(input.ToDate.HasValue, e => e.PaymentTime.Date <= input.ToDate.Value.AddDays(1))
                    .WhereIf(input.MachineFilter.HasValue, e => e.MachineId.Equals(input.MachineFilter));


                if (input.TransactionType != -1)
                {
                    if (input.TransactionType == 1)
                    {
                        transactions = transactions.Where(e => e.Status == TransactionStatus.Success);
                    }
                    else
                    {
                        transactions = transactions.Where(e => e.Status != TransactionStatus.Success);
                        transactions = transactions.WhereIf(!string.IsNullOrWhiteSpace(input.StateFilter), e => e.Status.ToString().Equals(input.StateFilter));
                    }
                }


                transactions = transactions
                   .Include(x => x.Machine)
                   //.Include(x => x.Session)
                   .Include(x => x.Inventories)
                   .Include(x => x.CashlessDetail)

                   .Include("Inventories.Product")
                   .Include("Inventories.Product.ProductCategoryRelations.ProductCategory");

                var totalCount = await transactions.CountAsync();
                var tranLists = await transactions.OrderBy(input.Sorting ?? "PaymentTime desc")
                    .PageBy(input)
                    .ToListAsync();

                var list = new List<TransactionItemsReportDto>();
                foreach (var x in tranLists)
                {
                    var inventories = x.Inventories;
                    foreach (var inven in inventories)
                    {
                        var cateName = inven.Product.ProductCategoryRelations.Select(c => c.ProductCategory.Name).ToList();
                        var cate = string.Join(", ", cateName);
                        var newTran = new TransactionItemsReportDto()
                        {
                            // Master data
                            Machine = x.Machine == null ? null : x.Machine.Name,
                            TransactionId = x.Machine == null ? x.LocalTranId.ToString() : $"{x.Machine.Name}_{x.LocalTranId.ToString()}",
                            DateTime = x.PaymentTime,
                            TransactionStatus = x.Status.ToString(),
                            PaymentType = x.CashlessDetail.CardLabel,
                            CardId = x.CashlessDetail.CardNumber,
                            AmountPaid = x.Amount,

                            // Detail data
                            Category = cate,
                            ProductName = inven.Product.Name,
                            ExpireDate = "",
                            Sku = inven.Product.SKU,
                            TagId = inven.TagId,
                            ProductUnitPrice = inven.Product.Price,
                            ProductDiscountPrice = 0,

                        };

                        list.Add(newTran);
                    }

                }
                detailLogService.Log($"GetAllTransactionItemsReport returns : {list.Count}");
                return new PagedResultDto<TransactionItemsReportDto>(totalCount, list);
            }
            catch (Exception ex)
            {
                Logger.Error($"GetAllTransactionItemsReport {ex.Message}", ex);
                return new PagedResultDto<TransactionItemsReportDto>(0, new List<TransactionItemsReportDto>());
            }
        }


        [AbpAllowAnonymous]
        public async Task<bool> SyncTransaction(DetailTransaction tran, Guid machineId)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                    {
                        Logger.Info("Start sync transaction web api");

                        int? tenantId = null;
                        var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineId);

                        if (machine != null)
                        {
                            tenantId = machine.TenantId;
                        }
                        else
                        {
                            Logger.Error("Start sync transaction web api | Machine not found: " + machineId);
                            return false;
                        }
                        //var tran = JsonConvert.DeserializeObject<DetailTransaction>(keyValueMessage.JsonValue);
                        tran.LocalTranId = tran.Id;
                        tran.MachineId = machineId;
                        tran.TenantId = tenantId;

                        var inventoryIds = tran.Inventories.Select(x => x.Id).ToList();
                        var inventories = tran.Inventories;

                        //calculate Sold for Topup
                        var topup = await _topupRepository.GetAll().Where(x => x.EndDate == null && x.MachineId == machineId)
                            .OrderByDescending(x => x.StartDate).FirstOrDefaultAsync();

                        Logger.Info($"Inventory {JsonConvert.SerializeObject(tran.Inventories)}");
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
                                MachineId = tran.MachineId.ToString(),
                                LocalInventoryId = inventory.Id.ToString(),
                                TransactionId = tran.Id,
                                ProductId = inventory.ProductId
                            };
                            newTransactionDetail.Add(transDetail);
                        }
                        Logger.Info($"TransactionDetail {JsonConvert.SerializeObject(newTransactionDetail)}");
                        /// ////////////////////////////////////
                        /// End
                        /// Description: uppate data into [TransactionDetail] table 
                        /// ////////////////////////////////////

                        tran.Id = 0;
                        tran.Inventories = newInventories;
                        tran.TransactionDetails = newTransactionDetail;

                        if (tran.Status == Enums.TransactionStatus.Success)
                        {
                            if (topup != null)
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

                        await _tranRepository.InsertAsync(tran);

                        if (topup != null)
                        {
                            await _topupRepository.UpdateAsync(topup);
                        }

                        await unitOfWork.CompleteAsync();
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Sync Transaction Web Api", e);
                return false;
            }
        }

        [AbpAllowAnonymous]
        public async Task<List<long>> AddTransactions(IList<DetailTransaction> trans)
        {
            try
            {
                var successTrans = new List<long>();
                IQueryable<Machine> machinesQuery;
                IQueryable<Session> sessionsQuery;
                IQueryable<DetailTransaction> existTransQuery;
                Machine machine = null;
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
                {
                    var mId = trans[0].MachineId;
                    machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == mId);
                    if (machine == null)
                    {
                        Logger.Error($"Sync Transaction: MachineId: {mId} does not exist");
                        return null;
                    }
                    else if (machine.IsDeleted)
                    {
                        Logger.Error($"Sync Transaction: {machine.Name}-{mId} is deleted");
                        return null;
                    }

                    machinesQuery = _machineRepository.GetAll();
                    sessionsQuery = _sessionRepository.GetAll();
                    existTransQuery = _transactionRepository.GetAll();

                    foreach (var tran in trans)
                    {
                        detailLogService.Log($"{tran.MachineName}-Sync Transaction: {tran.ToString()}");
                        tran.LocalTranId = tran.Id;
                        var oldId = tran.Id;
                        tran.Id = 0;

                        //transaction existed
                        if (await existTransQuery.AnyAsync(x => x.LocalTranId == tran.LocalTranId && x.MachineId == tran.MachineId)) continue;

                        if (tran.MachineId != null)
                        {
                            if (!(await machinesQuery.AnyAsync(x => x.Id == tran.MachineId)))
                            {
                                tran.MachineId = null;
                            }
                        }
                        if (tran.SessionId != null)
                        {
                            if (!(await sessionsQuery.AnyAsync(x => x.Id == tran.SessionId)))
                            {
                                tran.SessionId = null;
                            }
                        }

                        tran.TenantId = machine?.TenantId;
                        await _transactionRepository.InsertAsync(tran);
                        successTrans.Add(oldId);
                        detailLogService.Log($"{tran.MachineName}-Sync Transaction-{tran.Id} added");
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                    return successTrans;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Add Transactions {ex.Message}", ex);
                return null;
            }
        }

        [AbpAllowAnonymous]
        public async Task<bool> BulkSyncTransaction(List<DetailTransaction> transactions, Guid machineId)
        {
            try
            {
                using (var unitOfWork = _unitOfWorkManager.Begin())
                {
                    using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                    {
                        Logger.Info("Start sync transaction web api");

                        int? tenantId = null;
                        var machine = await _machineRepository.FirstOrDefaultAsync(x => x.Id == machineId);

                        if (machine != null)
                        {
                            tenantId = machine.TenantId;
                        }
                        else
                        {
                            Logger.Error("Start sync transaction web api | Machine not found: " + machineId);
                            return false;
                        }

                        foreach (var tran in transactions)
                        {
                            tran.LocalTranId = tran.Id;
                            tran.MachineId = machineId;
                            tran.TenantId = tenantId;

                            var inventoryIds = tran.Inventories.Select(x => x.Id).ToList();
                            var inventories = tran.Inventories;

                            //calculate Sold for Topup
                            var topup = await _topupRepository.GetAll().Where(x => x.EndDate == null && x.MachineId == machineId)
                                .OrderByDescending(x => x.StartDate).FirstOrDefaultAsync();

                            Logger.Info($"Inventory {tran.Inventories}");
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

                            tran.Id = 0;
                            tran.Inventories = newInventories;

                            if (tran.Status == Enums.TransactionStatus.Success)
                            {
                                if (topup != null)
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

                            await _tranRepository.InsertAsync(tran);

                            if (topup != null)
                            {
                                await _topupRepository.UpdateAsync(topup);
                            }
                        }

                        await unitOfWork.CompleteAsync();
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Sync Transaction Web Api", e);
                return false;
            }
        }

    }
}