using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Transactions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace KonbiCloud.BackgroundJobs
{
    public class SyncTransactionJob : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IRepository<DetailTransaction, long> _transactionRepository;
        private readonly IDetailLogService detailLogService;
        private bool isRunning;
        private string _cloudUrl;
        private bool _useRabbitMqToSync;
        private bool _useCloud;

        public SyncTransactionJob(AbpTimer timer,
                                  IRepository<DetailTransaction, long> transactionRepository,
                                  IDetailLogService detailLog,
                                  ISettingManager settingManager) : base(timer)
        {
            Timer.Period = 24 * 60 * 60 * 1000; //1 day
            //Timer.Period = 60 * 1000; //1 day
            _transactionRepository = transactionRepository;
            this.detailLogService = detailLog;
            _cloudUrl = settingManager.GetSettingValue(AppSettingNames.CloudApiUrl);

            bool.TryParse(settingManager.GetSettingValue("RfidFridgeSetting.System.Cloud.SyncUseRabbitMq"), out _useRabbitMqToSync);

            if (!_cloudUrl.EndsWith("/"))
            {
                _cloudUrl += "/";
            }

            bool.TryParse(settingManager.GetSettingValue(AppSettingNames.UseCloud), out _useCloud);
        }

        [UnitOfWork]
        protected override async void DoWork()
        {
            if(SettingManager == null)
            {
                detailLogService.Log($"Sync Transaction: SettingManager is null");
                return;
            }
            if (isRunning) return;
            isRunning = true;

            try
            {
                var _machineId = Guid.Empty;
                Guid.TryParse(SettingManager.GetSettingValue(AppSettingNames.MachineId), out _machineId);
                if (_machineId == Guid.Empty)
                {
                    detailLogService.Log($"Sync Transaction: Machine Id is null");
                    isRunning = false;
                    return;
                }

                using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant, AbpDataFilters.MustHaveTenant))
                {
                    var unSyncTrans = _transactionRepository.GetAllIncluding()
                                                            .WhereIf(true, x => !x.IsSynced)
                                                            .Include(x => x.Products)
                                                            .Include(x => x.Inventories)
                                                            .Include(x => x.CashlessDetail)
                                                            .Include(x => x.Topup)
                                                            .ToList();

                    if (unSyncTrans.Any())
                    {
                        detailLogService.Log($"Start push {unSyncTrans.Count()} transactions to server");

                        this.detailLogService.Log("Sync one transaction using WebApi");

                        var url = $"{_cloudUrl}api/services/app/Transaction/BulkSyncTransaction?machineId={_machineId}";
                        this.detailLogService.Log("Transaction SYNC Url: " + url);

                        using (var httpClient = new HttpClient())
                        {
                            using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                            {
                                request.Headers.TryAddWithoutValidation("accept", "text/plain");

                                var transactionJson = JsonConvert.SerializeObject(unSyncTrans);
                                this.detailLogService.Log("Transaction SYNC JSON: " + transactionJson);

                                request.Content = new StringContent(transactionJson);
                                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json-patch+json");

                                var response = await httpClient.SendAsync(request);
                                this.detailLogService.Log("Transaction SYNC response: " + response);

                                var result = response.Content;
                                //if (response.Content.Headers. == false)
                                //{
                                //    foreach(var transaction in unSyncTrans)
                                //    {
                                //        transaction.IsSynced = true;
                                //        await _transactionRepository.UpdateAsync(transaction);
                                //    }
                                //}

                                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                {
                                    this.detailLogService.Log("Synced to cloud OK marking local transaction as Synced");

                                    foreach (var transaction in unSyncTrans)
                                    {
                                        transaction.IsSynced = true;
                                        await _transactionRepository.UpdateAsync(transaction);
                                    }
                                }
                            }
                        }
                    }

                    isRunning = false;
                }
            }
            catch (Exception ex)
            {
                isRunning = false;
                Logger.Error($"Push Transactions result: {ex.Message}", ex);
            }
            finally
            {
                isRunning = false;
            }

        }
    }
}
