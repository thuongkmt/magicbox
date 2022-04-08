using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Machines;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Services;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace KonbiCloud.BackgroundJobs
{
    public class RefreshCacheJob : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IDetailLogService _detailLogService;
        private readonly ICacheManager _cacheManager;
        private readonly ILogger _logger;
        private readonly ISendMessageToMachineClientService _sendMessageToMachineService;

        public RefreshCacheJob(
            AbpTimer timer,
            IRepository<Machine, Guid> machineRepository,
            IDetailLogService detailLogService,
            ICacheManager cacheManager, 
            ILogger logger,
            ISendMessageToMachineClientService sendMessageToMachineService
        ) : base(timer)
        {
            Timer.Period = 60 * 1000 * 60; //1 hour
            _detailLogService = detailLogService;
            _machineRepository = machineRepository;
            _cacheManager = cacheManager;
            _logger = logger;
            _sendMessageToMachineService = sendMessageToMachineService;
        }


        [UnitOfWork]
        protected override void DoWork()
        {
            try
            {
                //using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                //{
                //    var cache = _cacheManager.GetCache(Const.ProductTagRealtime);
                //    var machines = _machineRepository.GetAll().ToList();

                //    foreach(var item in machines)
                //    {
                //        var cacheData = cache.Get(item.Id.ToString(), () => null);

                //        if(cacheData != null)
                //        {
                //            var jsonValue = JsonConvert.SerializeObject(cacheData);
                //            _detailLogService.Log("Inventory cache of machine : " + item.Name + " " + jsonValue);

                //            _cacheManager.GetCache(Common.Const.ProductTagRealtime)
                //                       .SetAsync(item.Id.ToString(), cacheData);
                //            _detailLogService.Log("Update cache successfully");
                //        }
                //    }
                //}

                _sendMessageToMachineService.SendQueuedMsgToMachines(new KeyValueMessage
                {
                    Key = MessageKeys.SyncInventoriesToCloud
                }, CloudToMachineType.AllMachines);

                _detailLogService.Log("Send message to machine to refresh cache");
            }
            catch(Exception ex)
            {
                _logger.Error($"Error when refresh cache: " + ex.Message);
            }
        }
    }
}
