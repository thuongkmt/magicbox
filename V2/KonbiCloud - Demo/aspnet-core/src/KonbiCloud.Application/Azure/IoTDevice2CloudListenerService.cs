//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Abp.AutoMapper;
//using Abp.Dependency;
//using Abp.Domain.Repositories;
//using Abp.Domain.Uow;
//using Castle.Core.Logging;
//using KonbiCloud.BlackVMCDiagnostic;
//using KonbiCloud.Common;
//using KonbiCloud.Configuration;
//using KonbiCloud.Diagnostic;
//using KonbiCloud.Diagnostic.Dtos;
//using KonbiCloud.RedisCache;
//using Microsoft.Azure.EventHubs;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Options;
//using Newtonsoft.Json;

//namespace KonbiCloud.Azure
//{
//    public interface IIoTDevice2CloudListenerService : ISingletonDependency, IDisposable
//    {
//        Task ExecuteReceiveMessages();
//    }

//    public class IoTDevice2CloudListenerService: IIoTDevice2CloudListenerService
//    {
//        private readonly AzureIoTHubOption _azOption;
//        private static IAzureRedisService _redisService;
//        private static ILogger _logger;
//        private readonly IBlackVMCDiagnosticAppService _blackVmcDiagnosticAppService;
//        private readonly IRepository<HardwareDiagnostic, long> _hardwareDiagnosticRepository;
//        private readonly IRepository<HardwareDiagnosticDetail, long> _hardwareDiagnosticDetailRepository;
//        private readonly IUnitOfWorkManager _manager;
//        //private readonly string _redisStorageString;
//        private IDictionary<string, PartitionReceiver> partitionReceivers=new Dictionary<string, PartitionReceiver>();


//        public IoTDevice2CloudListenerService(IOptions<AzureIoTHubOption> azOption,
//            IAzureRedisService redisService,
//            ILogger logger,
//            IBlackVMCDiagnosticAppService blackVmcDiagnosticAppService,
//            IRepository<HardwareDiagnostic, long> hardwareDiagnosticRepository,
//            IRepository<HardwareDiagnosticDetail, long> hardwareDiagnosticDetailRepository,
//            IUnitOfWorkManager manager, IConfiguration configuration)
//        {
//            _redisService = redisService;
//            _logger = logger;
//            _blackVmcDiagnosticAppService = blackVmcDiagnosticAppService;
//            _hardwareDiagnosticRepository = hardwareDiagnosticRepository;
//            _hardwareDiagnosticDetailRepository = hardwareDiagnosticDetailRepository;
//            _manager = manager;
//            //_redisStorageString = configuration[AppSettingNames.AZURE_REDIS_STORAGE];
//            _azOption = azOption.Value;

//            // Create an EventHubClient instance to connect to the
//            // IoT Hub Event Hubs-compatible endpoint.
//            var connectionString = new EventHubsConnectionStringBuilder(_azOption.EventHubsCompatibleEndpoint);
//            _eventHubClient = EventHubClient.CreateFromConnectionString(connectionString.ToString());
//        }
//        private static EventHubClient _eventHubClient;

//        public async Task ExecuteReceiveMessages()
//        {
//            try
//            {
//                // Create a PartitionReciever for each partition on the hub.
//                var runtimeInfo = await _eventHubClient.GetRuntimeInformationAsync();
//                var d2CPartitions = runtimeInfo.PartitionIds;

//                var tasks = new List<Task>();
//                foreach (string partition in d2CPartitions)
//                {
//                    tasks.Add(ReceiveMessagesFromDeviceAsync(partition));
//                }

//                // Wait for all the PartitionReceivers to finsih.
//                Task.WaitAll(tasks.ToArray());

//            }
//            catch (Exception e)
//            {
//                _logger.Error(e.Message, e);
//            }

//        }

//        private PartitionReceiver GetPartitionReceiver(string partition)
//        {
//            var existed = partitionReceivers.ContainsKey(partition) ? partitionReceivers[partition] : null;
//            if (existed == null)
//            {
//                // Create the receiver using the default consumer group.
//                // For the purposes of this sample, read only messages sent since 
//                // the time the receiver is created. Typically, you don't want to skip any messages.
//                existed = _eventHubClient.CreateReceiver("$Default", partition,
//                    EventPosition.FromEnqueuedTime(DateTime.Now));
//                Console.WriteLine("Create receiver on partition: " + partition);
//                partitionReceivers.Add(partition,existed);
//            }
//            return existed;
//        }

//        // Asynchronously create a PartitionReceiver for a partition and then start 
//        // reading any messages sent from the simulated client.
//        private async Task ReceiveMessagesFromDeviceAsync(string partition)
//        {

//            var eventHubReceiver = GetPartitionReceiver(partition);
//            _logger.Info("Listening for messages on: " + partition);
//            // Check for EventData - this methods times out if there is nothing to retrieve.
//            var events = await eventHubReceiver.ReceiveAsync(100);

//            // If there is data in the batch, process it.
//            if (events == null) return;

//            foreach (EventData eventData in events)
//            {
//                try
//                {
//                    string data = Encoding.UTF8.GetString(eventData.Body.Array);
//                    _logger.Info($"Message received on partition {partition}:");
//                    _logger.Info($"  {data}:");
//                    _logger.Info("Application properties (set by device):");
//                    foreach (var prop in eventData.Properties)
//                    {
//                        _logger.Info($"  {prop.Key}: {prop.Value}");
//                    }
//                    _logger.Info("System properties (set by IoT Hub):");
//                    foreach (var prop in eventData.SystemProperties)
//                    {
//                        _logger.Info($"  {prop.Key}: {prop.Value}");
//                    }
//                    var msgData = data.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
//                    var cmd = (IoTHubCommands)Enum.Parse(typeof(IoTHubCommands), msgData[1]);
//                    //var date = DateTime.Parse(msgData[2]);
//                    var deviceId = eventData.SystemProperties["iothub-connection-device-id"].ToString();
//                    await ProcessDeviceCommand(deviceId, cmd, msgData[3]);
//                }
//                catch (Exception e)
//                {
//                    _logger.Error(e.Message);
//                }

//            }

//        }

//        private async Task ProcessDeviceCommand(string deviceId, IoTHubCommands cmd, string cmdObj = null)
//        {
//            switch (cmd)
//            {
//                case IoTHubCommands.CheckMachineOnline:
//                    break;
//                case IoTHubCommands.D2CUpdateMachineStatus:
//                    UpdateVendingMachineStatus(cmdObj);
//                    break;
//                case IoTHubCommands.D2CVMCStatus:
//                    await UpdateMachineStatus(deviceId, cmdObj);
//                    break;
//                case IoTHubCommands.D2CHardwareDiagnostic:
//                    await UpdateHardwareDiagnostic(cmdObj);
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(cmd), cmd, null);
//            }
//        }

//        private void UpdateVendingMachineStatus(string cmdObj)
//        {
//            var machine = JsonConvert.DeserializeObject<VendingMachineStatusCache>(cmdObj);
//            var cache = _redisService.Get<List<VendingMachineStatusCache>>(VendingMachineStatusCache.GetListCacheName()) ?? new List<VendingMachineStatusCache>();
//            if (machine != null)
//            {
//                if (cache.Any(x => x.MachineId == machine.MachineId))
//                {
//                    var tmp = cache.FirstOrDefault(x => x.MachineId == machine.MachineId);
//                    cache.Remove(tmp);
//                }
//                machine.LastModified = DateTime.Now;
//                cache.Add(machine);
//                _redisService.Set(cache, VendingMachineStatusCache.GetListCacheName());
//            }
//        }

//        private async Task UpdateHardwareDiagnostic(string cmdObj)
//        {
//            var diagnosticDto = JsonConvert.DeserializeObject<HardwareDiagnosticFromClientDto>(cmdObj);
//            var diagnostic = diagnosticDto.MapTo<HardwareDiagnostic>();
//            await _hardwareDiagnosticRepository.InsertAsync(diagnostic);
//            await _manager.Current.SaveChangesAsync();
//            foreach (var hardwareDiagnosticDetail in diagnosticDto.Details)
//            {
//                await _hardwareDiagnosticDetailRepository
//                    .InsertAsync(hardwareDiagnosticDetail.MapTo<HardwareDiagnosticDetail>());
//            }
//            await _manager.Current.SaveChangesAsync();
//        }
//        private async Task UpdateMachineStatus(string machineId, string cmdObj)
//        {
//            var state = JsonConvert.DeserializeObject<MachineStatus>(cmdObj);
//            state.MachineId = machineId;
//            state.LogTime = DateTime.Now;
//            await _blackVmcDiagnosticAppService.AddOrUpdateMachineStatus(state);
//        }

//        public void Dispose()
//        {
//            _eventHubClient?.Close();
//        }
//    }
//}

