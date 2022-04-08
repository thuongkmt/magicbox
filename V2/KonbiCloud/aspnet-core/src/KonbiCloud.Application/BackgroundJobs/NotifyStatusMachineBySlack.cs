using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Runtime.Caching;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.EntityFrameworkCore;
using KonbiCloud.Machines;
using KonbiCloud.Machines.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KonbiCloud.BackgroundJobs
{
    public class NotifyStatusMachineBySlack : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ICacheManager _cacheManager;
        private readonly ILogger _logger;
        private readonly ISlackService _slackService;
        private readonly IRepository<Machine, Guid> _machineRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        Dictionary<string, bool> historyMachineStatus = new Dictionary<string, bool>();
        public NotifyStatusMachineBySlack(AbpTimer timer,
            ICacheManager cacheManager,
            ILogger logger,
            ISlackService slackService,
            IRepository<Machine, Guid> machineRepository,
            IUnitOfWorkManager unitOfWorkManager) : base(timer)
        {
            Timer.Period = 15000; //check connection every 2 minutes!
            _cacheManager = cacheManager;
            _logger = logger;
            _slackService = slackService;
            _machineRepository = machineRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }


        [UnitOfWork]

        protected override void DoWork()
        {
            CheckSendNotifyBySlack().Wait();
        }

        bool isRunning = false;
        private async Task CheckSendNotifyBySlack()
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var msDtos = new List<MachineStatusDto>();
                try
                {
                    var machines = _machineRepository.GetAll().ToList();
                    var cacheItem = _cacheManager.GetCache(Const.MachineStatus);

                    bool machineHealth = true;
                    foreach (var mc in machines)
                    {
                        var mcInCache = (MachineStatusDto)cacheItem.Get(mc.Id.ToString(), () => null);
                        if (mcInCache == null)
                        {
                            _logger.Info($"BackgroundJob.NotifyStatusMachine: not found any cache");
                        }

                        ///1. OFF-LINE CASE
                        if (mcInCache != null)
                        {
                            if (Clock.Now.Subtract(mcInCache.LastUpdate).TotalMinutes > 5)
                       {
                                machineHealth = false;
                                if (!historyMachineStatus.ContainsKey(mc.Id.ToString()))
                                {
                                    historyMachineStatus.Add(mc.Id.ToString(), false);
                                }
                                cacheItem.Remove(mc.Id.ToString());
                            }
                        }
                        
                        //NOTIFY TO SALCK that machine is offline
                        if (!machineHealth)
                        {
                            await NotifyMachineStatus(mc.Name, "Offline");
                            continue;
                        }

                        /// 2. ONLINE CASE
                        //NOTIFY TO SLACK that machine online again
                        if (historyMachineStatus.Count > 0)
                        {
                            foreach (var hMachineStatus in historyMachineStatus)
                            {
                                if (hMachineStatus.Key == mcInCache?.MachineId.ToString())
                                {
                                    await NotifyMachineStatus(mc.Name, "Online");
                                    historyMachineStatus.Remove(mc.Id.ToString());
                                }

                                
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    _logger.Error("Error Get All Machine Status", e);
                }
            };


        }

        private async Task NotifyMachineStatus(string machineName, string status)
        {
            var channelName = await SettingManager.GetSettingValueAsync(AppSettings.Slack.ChannelName);
            //if tenant register to us, then we create a channel for them if not will ignore this notification
            if (channelName != null)
            {
                await _slackService.SendAlert(machineName, status, channelName);
            }
        }
    }
}
