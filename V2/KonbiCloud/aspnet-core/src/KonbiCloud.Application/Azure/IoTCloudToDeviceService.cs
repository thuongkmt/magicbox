//using System;
//using System.Text;
//using System.Threading.Tasks;
//using Abp.Configuration;
//using Abp.Dependency;
//using Castle.Core.Logging;
//using KonbiCloud.Common;
//using Konbini.KonbiCloud.Azure;

//using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json;

//namespace KonbiCloud.Azure
//{
//    public class IoTCloudToDeviceService : ISingletonDependency, IIoTCloudToDeviceService  
//    {
//        private readonly ISettingManager _settingManager;
//        private readonly ILogger _logger;
//        private ServiceClient _serviceClient;
//        private readonly RegistryManager _registryMgr;
//        private readonly IConfiguration configuration;
//        private readonly string iotHubConnectionString;

//        public IoTCloudToDeviceService(ISettingManager settingManager,
//            ILogger logger, IConfiguration configuration)
//        {
//            this.configuration = configuration;
//            try
//            {
//                _settingManager = settingManager;
//                _logger = logger;
//                iotHubConnectionString = configuration["AzureIoTHubOption:IoTHubConnectionStr"];
//                _serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
//                _registryMgr = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
//            }
//            catch (Exception e)
//            {
//                logger.Error(e.Message,e);
//            }
//        }

//        /// <summary>
//        /// Send command enum to device
//        /// </summary>
//        /// <param name="machineId">Machine that receive the command</param>
//        /// <param name="command">command enum (int)</param>
//        /// <param name="commandObject">Command object, convert to json</param>
//        /// <returns>True if machine acknowledge receive command, otherwise false</returns>
//        public async Task<bool> SendDevice(string machineId, IoTHubCommands command, dynamic commandObject = null)
//        {
//            try
//            {
//                var device = await _registryMgr.GetDeviceAsync(machineId);
//                if (device == null)
//                {
//                    //@todo:send to slack alert
//                    _logger.Info($"Couldn't find device {machineId} in IoTHub");
//                    return false;
//                }

//                var cmdObjJson = commandObject != null ? JsonConvert.SerializeObject(commandObject) : "";
//                var msg = new Message(
//                    Encoding.UTF8.GetBytes($"Command::{command}::{DateTime.Now:yyyyMMddHHmmss}::{cmdObjJson}"));
//                msg.Ack = DeliveryAcknowledgement.Full;

//                await _serviceClient.SendAsync(device.Id, msg);

//                //var feedbackReceiver = _serviceClient.GetFeedbackReceiver();
//                //var feedbackBatch = await feedbackReceiver.ReceiveAsync(TimeSpan.FromSeconds(60));      //time out is 60 seconds
//                //if (feedbackBatch == null)
//                //{
//                //    //await _serviceClient.CloseAsync();
//                //    return false;
//                //}

//                //await feedbackReceiver.CompleteAsync(feedbackBatch);
//                ////await _serviceClient.CloseAsync();
//                return true;

//            }
//            catch (Exception exp)
//            {
//                System.Diagnostics.Debug.WriteLine($"Exception:{exp.Message}");
//                return false;
//            }
//        }

//        public async Task<Device> CheckDeviceOnline(string machineId)
//        {
//            try
//            {
//                if (_serviceClient == null)
//                    _serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);


//                var registryMgr = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
//                var device = await registryMgr.GetDeviceAsync(machineId);
//                return device;
//            }
//            catch (Exception exp)
//            {
//                System.Diagnostics.Debug.WriteLine($"Exception:{exp.Message}");
//                return null;
//            }
//        }

//        public void Dispose()
//        {
//            _serviceClient?.Dispose();
//        }
//    }
//}
