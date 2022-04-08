//using System;
//using System.Threading.Tasks;
//using Abp.Dependency;
//using Abp.Domain.Repositories;
//using Abp.Threading.BackgroundWorkers;
//using Abp.Threading.Timers;
//using Castle.Core.Logging;
//using KonbiCloud.Common;
//using KonbiCloud.Machines;
//using KonbiCloud.RedisCache;


//namespace KonbiCloud.Azure
//{
//    public class MachineOfflineCheckingJob : PeriodicBackgroundWorkerBase, ISingletonDependency
//    {
//        private readonly ILogger _logger;
//        private readonly ISlackService _slackService;
//        private readonly IIoTCloudToDeviceService _iotHubService;
//        private readonly IRepository<Machine, Guid> machineRepository;
//        private readonly IAzureRedisService azureRedisService;
//        private static readonly object _lockObj = new object();
//        private readonly IIoTDevice2CloudListenerService ioTDevice2CloudListenerService;


//        public MachineOfflineCheckingJob(AbpTimer timer,
//            ILogger logger,
//            ISlackService slackService,
//            IIoTCloudToDeviceService iotHubService,
//            IRepository<Machine, Guid> machineRepository, IAzureRedisService azureRedisService, IIoTDevice2CloudListenerService ioTDevice2CloudListenerService) : base(timer)
//        {
//            _logger = logger;
//            _slackService = slackService;
//            _iotHubService = iotHubService;
//            Timer.Period = 30 * 1000; //30 seconds
//            this.machineRepository = machineRepository;
//            this.azureRedisService = azureRedisService;
//            this.ioTDevice2CloudListenerService = ioTDevice2CloudListenerService;
//        }

//        protected override async void DoWork()
//        {
//            //lock (_lockObj)
//            //{
//            try
//            {
//                var machines = azureRedisService.Get<MachineListCache>(MachineListCache.GetCacheName());
//                if (machines == null) return;
//                var isStatusChange = false;
//                for (int i = 0; i < machines.Items.Count; i++)
//                {
//                    if (await CheckMachine(machines.Items[i])) isStatusChange = true;
//                }
//                if(isStatusChange) azureRedisService.UpdateMachineList(machines.Items);

//                //check iot hubs
//                await ioTDevice2CloudListenerService.ExecuteReceiveMessages();
//            }
//            catch (Exception e)
//            {
//                _logger.Error(e.Message);
//            }
//            //}

//        }



//        private async Task<bool> CheckMachine(MachineCacheItem machine)

//        {
//            var device = await _iotHubService.CheckDeviceOnline(machine.MachineId.ToString());
//            var machineIsOnline = device != null && device.ConnectionState == DeviceConnectionState.Connected;
//            if (!machineIsOnline)
//            {
//                if (!machine.IsOffline)
//                {
//                    //_slackService.SendLenovoAlert(machine.Name,
//                    //    $"The machine is offline! Please check internet connection.");
//                    _slackService.SendLenovoAlert(machine.LogicalId,
//                        $"The machine is  *`offline`*:scream:! Please check internet connection.");
//                    machine.IsOffline = true;
//                    return true;
//                }
//            }
//            else if (machine.IsOffline) // machine online and machine state is offline
//            {
//                //_slackService.SendLenovoAlert(machine.Name,
//                //    $"The machine is back to online.");

//                _slackService.SendLenovoAlert(machine.LogicalId,
//                    $"The machine is online:+1:.");
//                machine.IsOffline = false;
//                //azureRedisService.UpdateMachine(machine);
//                return true;
//            }

//            return false;
//        }
//    }
//}
