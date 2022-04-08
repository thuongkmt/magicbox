
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using KonbiCloud.Authorization;
using KonbiCloud.Configuration;
using KonbiCloud.Transactions.Dtos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Domain.Uow;
using KonbiCloud.Enums;
using System.IO;
using Abp.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using KonbiCloud.Common;
using KonbiCloud.Credit;
using Konbini.Messages;
using Konbini.Messages.Enums;

//using ITransactionSyncService = KonbiCloud.CloudSync.ITransactionSyncService;

namespace KonbiCloud.Transactions
{
    //using KonbiCloud.CloudSync;
    using KonbiCloud.Inventories;
    using KonbiCloud.Products.Dtos;
    using Konbini.Messages.Services;

    using Newtonsoft.Json;
    using System.Net.Http;
    using System.Net.Http.Headers;

    [AbpAuthorize(AppPermissions.Pages_Transactions)]
    public class TransactionAppService : KonbiCloudAppServiceBase, ITransactionAppService
    {
        private readonly IRepository<DetailTransaction, long> _transactionRepository;
        private readonly IRepository<InventoryItem, Guid> _inventoryReponsitory;
        private readonly IRepository<Topup, Guid> _topupRepository;
        private readonly IRepository<UserCredit, Guid> _userCrediRepository;
        private readonly IRepository<CreditHistory, Guid> _creditHistoryRepository;
        private readonly bool _useCloud;
        private readonly ISendMessageToCloudService _sendMessageToCloudService;

        //private readonly ITransactionSyncService transactionSyncService;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IDetailLogService detailLogService;
        private const string defaultImage = "assets/common/images/ic_nophoto.jpg";
        private const string ServerRootAddress = "App:ServerRootAddress";
        private readonly Guid _machineId;
        private string _cloudUrl;
        private bool _useRabbitMqToSync;

        public TransactionAppService(IRepository<DetailTransaction, long> transactionRepository,
                                     //ITransactionSyncService transactionSyncService,
                                     IHostingEnvironment env,
                                     IDetailLogService detailLog,
                                     IRepository<InventoryItem, Guid> inventoryReponsitory,
                                     IRepository<Topup, Guid> topupRepository,
                                     ISendMessageToCloudService sendMessageToCloudService,
                                     ISettingManager settingManager,
                                     IRepository<UserCredit, Guid> userCrediRepository,
                                     IRepository<CreditHistory, Guid> creditHistoryRepository)
        {
            _transactionRepository = transactionRepository;
            //this.transactionSyncService = transactionSyncService;
            _appConfiguration = env.GetAppConfiguration();
            this.detailLogService = detailLog;
            this._inventoryReponsitory = inventoryReponsitory;
            this._topupRepository = topupRepository;
            this._creditHistoryRepository = creditHistoryRepository;
            this._userCrediRepository = userCrediRepository;

            _sendMessageToCloudService = sendMessageToCloudService;
            _machineId = Guid.Parse(settingManager.GetSettingValue(AppSettingNames.MachineId));
            _cloudUrl = settingManager.GetSettingValue(AppSettingNames.CloudApiUrl);
            bool.TryParse(settingManager.GetSettingValue("RfidFridgeSetting.System.Cloud.SyncUseRabbitMq"), out _useRabbitMqToSync);

            if (!_cloudUrl.EndsWith("/"))
            {
                _cloudUrl += "/";
            }
            bool.TryParse(settingManager.GetSettingValue(AppSettingNames.UseCloud), out _useCloud);
        }

        public async Task<Topup> GetCurrentTopup()
        {
            var currentTopup = await _topupRepository.GetAll().Where(x => !x.EndDate.HasValue)
                                   .OrderByDescending(x => x.StartDate).FirstOrDefaultAsync();
            if (currentTopup == null)
                throw new UserFriendlyException("Can not find current top-up, please try to register new top-up.");

            return currentTopup;
        }


        [AbpAllowAnonymous]
        public async Task AddTransaction(FridgeTransactionDto transactionDto)
        {
            var inventoryList = transactionDto.Inventories;
            var ids = inventoryList.Select(x => x.Id).ToList();
            var data = this._inventoryReponsitory.GetAll().Where(x => ids.Contains(x.Id)).ToList();
            var inventories = new List<InventoryItem>();

            var json = JsonConvert.SerializeObject(transactionDto);
            this.detailLogService.Log("Add transaction | FridgeTransactionDto = " + json);


            double amount = 0;
            TransactionStatus status = TransactionStatus.Success;

            //if (transactionDto.Status == TransactionStatus.Success)
            //{

            //}
            //else
            //{
            //    status = transactionDto.Status;
            //}

            if (inventoryList.Count > 0)
            {
                foreach (var inventoryItem in data)
                {
                    inventoryItem.State = TagState.Sold;
                    inventories.Add(inventoryItem);
                    await _inventoryReponsitory.UpdateAsync(inventoryItem);//change state to sold
                }
                amount = inventories.Sum(x => x.Price);
                status = transactionDto.Status;//TransactionStatus.Success;
            }
            else
            {
                status = TransactionStatus.Cancelled;
            }

            if (transactionDto.Status == TransactionStatus.Test)
            {
                status = TransactionStatus.Test;
            }

            this.detailLogService.Log("Add transaction | amount = " + amount);

            var cashless = new CashlessDetail
            {
                Aid = transactionDto.CashlessDetail.Aid,
                Amount = transactionDto.CashlessDetail.Amount,
                AppLabel = transactionDto.CashlessDetail.AppLabel,
                ApproveCode = transactionDto.CashlessDetail.ApproveCode,
                Batch = transactionDto.CashlessDetail.Batch,
                CardLabel = transactionDto.CashlessDetail.CardLabel,
                CardNumber = transactionDto.CashlessDetail.CardNumber,
                EntryMode = transactionDto.CashlessDetail.EntryMode,
                Invoice = transactionDto.CashlessDetail.Invoice,
                Mid = transactionDto.CashlessDetail.Mid,
                Tid = transactionDto.CashlessDetail.Tid,
                Rrn = transactionDto.CashlessDetail.Rrn,
            };

            var transaction = new DetailTransaction
            {
                Inventories = inventories,
                Topup = await this.GetCurrentTopup(),
                Amount = decimal.Parse(amount.ToString()),
                PaymentTime = DateTime.Now,
                Status = status,
                CashlessDetail = cashless,
                OtherInfo1 = transactionDto.PaymentDetail,
                IsSynced = true
            };

            json = JsonConvert.SerializeObject(inventories);
            this.detailLogService.Log("Add transaction | Inventories = " + json);
            json = JsonConvert.SerializeObject(transaction);
            this.detailLogService.Log("Add transaction | Transaction = " + json);

            var newTran = await _transactionRepository.InsertAsync(transaction);
            await CurrentUnitOfWork.SaveChangesAsync();

            // TrungPQ: Check use Cloud.
            if (_useCloud)
            {
                try
                {
                    if (_useRabbitMqToSync)
                    {
                        this.detailLogService.Log("Sync Transaction using RabbitMQ");
                        _sendMessageToCloudService.SendQueuedMsgToCloud(new KeyValueMessage()
                        {
                            Key = MessageKeys.Transaction,
                            MachineId = _machineId,
                            Value = newTran
                        });
                    }
                    else
                    {
                        this.detailLogService.Log("Sync Transaction using WebApi");

                        var url = $"{_cloudUrl}api/services/app/Transaction/SyncTransaction?machineId={_machineId}";
                        this.detailLogService.Log("Transaction SYNC Url: " + url);

                        using (var httpClient = new HttpClient())
                        {
                            using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                            {
                                request.Headers.TryAddWithoutValidation("accept", "text/plain");
                                // request.Headers.TryAddWithoutValidation("Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW4iLCJBc3BOZXQuSWRlbnRpdHkuU2VjdXJpdHlTdGFtcCI6IjJlOWQ3N2FmLTQwM2MtYzlkNS0zM2E0LTM5ZjMyMDU5Nzc3NiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiaHR0cDovL3d3dy5hc3BuZXRib2lsZXJwbGF0ZS5jb20vaWRlbnRpdHkvY2xhaW1zL3RlbmFudElkIjoiMSIsInN1YiI6IjIiLCJqdGkiOiI4MDQ2YzIxNy00ZDExLTQ2NmMtYjRmNS03NTI3MGQxZjdmYjQiLCJpYXQiOjE1ODQ5Mzg3MjUsInRva2VuX3ZhbGlkaXR5X2tleSI6ImZmMGFjNzlkLTVhNzMtNDM1ZC1iMjMwLWNjNjUwYzlmOGNkMyIsInVzZXJfaWRlbnRpZmllciI6IjJAMSIsIm5iZiI6MTU4NDkzODcyNSwiZXhwIjoxNTg1MDI1MTI1LCJpc3MiOiJLb25iaUNsb3VkIiwiYXVkIjoiS29uYmlDbG91ZCJ9.NBgrVNIg8rWCoBTOYoLywglOp0vh_Lw_j0c06COwfOU");

                                var newTranJson = JsonConvert.SerializeObject(newTran);
                                this.detailLogService.Log("Transaction SYNC JSON: " + newTranJson);

                                request.Content = new StringContent(newTranJson);
                                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json-patch+json");

                                var response = await httpClient.SendAsync(request);
                                this.detailLogService.Log("Transaction SYNC response: " + response);

                                // if not response 200 OK, mark it as not sync
                                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                                {
                                    try
                                    {
                                        var missedTran = await _transactionRepository.GetAsync(newTran.Id);
                                        missedTran.IsSynced = false;

                                        await _transactionRepository.UpdateAsync(missedTran);
                                        await CurrentUnitOfWork.SaveChangesAsync();
                                        this.detailLogService.Log("Add transaction to cloud ERROR, masked it as Synced = false: " + JsonConvert.SerializeObject(missedTran));
                                    }
                                    catch (Exception ex2)
                                    {
                                        this.detailLogService.Log("Add transaction to cloud ERROR, masked it as Synced = false ERROR: " + ex2.ToString());
                                    }
                                }
                            }
                        }
                    }

                    this.detailLogService.Log("Add transaction to cloud, machine id = " + _machineId);
                }
                catch (Exception ex)
                {
                    try
                    {
                        var missedTran = await _transactionRepository.GetAsync(newTran.Id);
                        missedTran.IsSynced = false;

                        await _transactionRepository.UpdateAsync(missedTran);
                        await CurrentUnitOfWork.SaveChangesAsync();
                        this.detailLogService.Log("Add transaction to cloud ERROR, masked it as Synced = false: " + JsonConvert.SerializeObject(missedTran));
                    }
                    catch (Exception ex2)
                    {
                        this.detailLogService.Log("Add transaction to cloud ERROR, masked it as Synced = false ERROR: " + ex2.ToString());
                    }

                    this.detailLogService.Log("Add transaction to cloud ERROR, machine id = " + ex.ToString());
                }
            }
            else
            {
                this.detailLogService.Log("Add transaction, machine id = " + _machineId);
            }



        }

        [AbpAuthorize(AppPermissions.Pages_Transactions)]
        public async Task<PagedResultDto<TransactionDto>> GetAllTransactions(TransactionInput input)
        {
            try
            {
                DateTime? fromDate = null;
                if (!string.IsNullOrEmpty(input.FromDate))
                {
                    fromDate = DateTime.ParseExact(input.FromDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                }

                DateTime? toDate = null;
                if (!string.IsNullOrEmpty(input.ToDate))
                {
                    toDate = DateTime.ParseExact(input.ToDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                }

                var transactions = _transactionRepository.GetAllIncluding()
                    .WhereIf(!string.IsNullOrWhiteSpace(input.SessionFilter), e => e.SessionId.ToString().Equals(input.SessionFilter))
                    .WhereIf(!string.IsNullOrEmpty(input.FromDate), e => e.PaymentTime.Date >= fromDate)
                    .WhereIf(!string.IsNullOrEmpty(input.ToDate), e => e.PaymentTime.Date <= toDate);

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
                   .Include(x => x.Inventories)
                   .Include(x => x.Session)
                   .Include(x => x.Dishes)
                   .Include(x => x.CashlessDetail)
                   .Include("Dishes.Disc")
                   .Include("Inventories.Product");

                var totalCount = await transactions.CountAsync();

                var tranList = await transactions.OrderBy(input.Sorting ?? "PaymentTime desc")
                    .PageBy(input)
                    .ToListAsync();

                var machineName = await SettingManager.GetSettingValueAsync(AppSettingNames.MachineName);
                var pathImage = Path.Combine(_appConfiguration[ServerRootAddress], Const.ImageFolder, _appConfiguration[AppSettingNames.TransactionImageFolder]);
                var list = new List<TransactionDto>();
                foreach (var x in tranList)
                {
                    var newTran = new TransactionDto()
                    {
                        Id = x.Id,
                        TranCode = x.TranCode.ToString(),
                        Buyer = x.Buyer,
                        PaymentTime = x.PaymentTime,
                        Amount = x.Amount,
                        PlatesQuantity = x.Dishes == null ? 0 : x.Dishes.Count,
                        States = x.Status.ToString(),
                        Dishes = ObjectMapper.Map<ICollection<DishTransactionDto>>(x.Dishes),
                        Inventories = x.Inventories,
                        InventoriesQuantity = x.Inventories == null ? 0 : x.Inventories.Count,

                        Session = x.Session?.Name,
                        TransactionId = string.IsNullOrEmpty(machineName) ? x.Id.ToString() : $"{machineName}_{x.Id}",
                        BeginTranImage = x.BeginTranImage,
                        EndTranImage = x.EndTranImage,
                        PaidAmount = x.CashlessDetail == null ? 0 : x.CashlessDetail.Amount,
                        CardLabel = x.CashlessDetail == null ? null : x.CashlessDetail.CardLabel,
                        CardNumber = x.CashlessDetail == null ? null : x.CashlessDetail.CardNumber,
                        OtherInfo1 = x.OtherInfo1,
                        IsSynced = x.IsSynced
                    };

                    if (string.IsNullOrEmpty(x.BeginTranImage))
                    {
                        newTran.BeginTranImage = defaultImage;
                    }
                    else
                    {
                        newTran.BeginTranImage = Path.Combine(pathImage, x.BeginTranImage);
                    }
                    if (string.IsNullOrEmpty(x.EndTranImage))
                    {
                        newTran.EndTranImage = defaultImage;
                    }
                    else
                    {
                        newTran.EndTranImage = Path.Combine(pathImage, x.EndTranImage);
                    }

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

        //Update Sync status after bg job is done
        [AbpAllowAnonymous]
        public async Task UpdateSyncStatus(IList<long> tranIds)
        {
            try
            {
                var existTrans = new List<DetailTransaction>();
                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant, AbpDataFilters.MayHaveTenant))
                {
                    existTrans = await _transactionRepository.GetAllListAsync();
                }

                foreach (var tranId in tranIds)
                {
                    var tran = existTrans.FirstOrDefault(x => x.Id == tranId);
                    if (tran != null)
                    {
                        tran.IsSynced = true;
                        tran.SyncDate = DateTime.Now;
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.Error($"Update transaction sync status {ex.Message}", ex);
            }
        }

        public async Task SyncTransactionToCloud(long id)
        {
            if (_useCloud)
            {
                var transaction = await _transactionRepository.GetAll()
                    .Include(x => x.Products)
                    .Include(x => x.Inventories)
                    .Include(x => x.CashlessDetail)
                    .Include(x => x.Topup)
                    .FirstOrDefaultAsync(x => x.Id == id);
                if (transaction == null) return;
        
                if (_useRabbitMqToSync)
                {
                    this.detailLogService.Log("Sync one transaction using RabbitMQ");

                    _sendMessageToCloudService.SendQueuedMsgToCloud(new KeyValueMessage()
                    {
                        Key = MessageKeys.ManuallySyncTransaction,
                        MachineId = _machineId,
                        Value = transaction
                    });
                }
                else
                {
                    this.detailLogService.Log("Sync one transaction using WebApi");

                    var url = $"{_cloudUrl}api/services/app/Transaction/SyncTransaction?machineId={_machineId}";
                    this.detailLogService.Log("Transaction SYNC Url: " + url);

                    using (var httpClient = new HttpClient())
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                        {
                            request.Headers.TryAddWithoutValidation("accept", "text/plain");

                            var transactionJson = JsonConvert.SerializeObject(transaction);
                            this.detailLogService.Log("Transaction SYNC JSON: " + transactionJson);

                            request.Content = new StringContent(transactionJson);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json-patch+json");

                            var response = await httpClient.SendAsync(request);
                            this.detailLogService.Log("Transaction SYNC response: " + response);

                            if(response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                transaction.IsSynced = true;
                                await _transactionRepository.UpdateAsync(transaction);
                            }
                        }
                    }
                }
            }
        }
    }
}