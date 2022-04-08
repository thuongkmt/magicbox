//using System;
//using Abp.Configuration;
//using Castle.Core.Logging;
//using KonbiCloud.Common;
//using KonbiCloud.Configuration;
//using Konbini.Messages;
//using MessagePack;
//using Microsoft.Extensions.Configuration;
//using Newtonsoft.Json;
//using RabbitMQ.Client;

//namespace KonbiCloud.Messaging
//{
//    public class RabbitMqSendMessageToCloudService : ISendMessageToCloudService
//    {
//        //private IConnection _connection;
//        //private IModel _queuedChannel;
//        //private IModel _noQueueChannel;
//        private ILogger _logger;

//        private readonly IConfigurationRoot _configurationRoot;
//        private readonly IDetailLogService _detailLogService;
//        private readonly string _machineId;
//        private readonly IConnectToRabbitMqService _connectToRabbitMqService;

//        public RabbitMqSendMessageToCloudService(IAppConfigurationAccessor configurationRoot,
//            IDetailLogService detailLogService,ILogger logger,ISettingManager settingManager, IConnectToRabbitMqService connectToRabbitMqService)
//        {
//            _machineId = settingManager.GetSettingValue(AppSettingNames.MachineId);
//            _configurationRoot = configurationRoot.Configuration;
//            _detailLogService = detailLogService;
//            _logger = logger;
//            _connectToRabbitMqService = connectToRabbitMqService;
//            _connectToRabbitMqService.Connect();

       
            

//        }

//        public bool SendQueuedMsgToCloud(KeyValueMessage message)
//        {
//            try
//            {
//                var _queuedChannel = _connectToRabbitMqService.GetQueuedModel();
//                _queuedChannel.QueueDeclare(queue: RabbitMqConstants.CLIENT_TO_SERVER_QUEUE,
//                    durable: true,
//                    exclusive: false,
//                    autoDelete: false,
//                    arguments: null);

//                var body = MessagePackSerializer.Serialize(message);

//                var properties = _queuedChannel.CreateBasicProperties();
//                properties.Persistent = true;

//                _queuedChannel.BasicPublish(exchange: "",
//                    routingKey: RabbitMqConstants.CLIENT_TO_SERVER_QUEUE,
//                    basicProperties: properties,
//                    body: body);
//                _detailLogService.Log($"Send message to cloud: {JsonConvert.SerializeObject(message)}");
//                return true;

//            }
//            catch (Exception e)
//            {
//               _logger.Error("SendQueuedMsgToCloud",e);
//               return false;
//            }
           

//        }

//        public bool SendMsgToCloud(KeyValueMessage message)
//        {
//            try
//            {
//                var _noQueueChannel = _connectToRabbitMqService.GetNoQueuedModel();
//                _noQueueChannel.QueueDeclare(durable: false);
//                _noQueueChannel.ExchangeDeclare(RabbitMqConstants.EXCHANGE_M2CLOUD_NOQUEUE, "fanout");
//                var body = MessagePackSerializer.Serialize(message);

//                _noQueueChannel.BasicPublish(exchange: RabbitMqConstants.EXCHANGE_M2CLOUD_NOQUEUE,
//                    routingKey: "",
//                    basicProperties: null,
//                    body: body);

//                _detailLogService.Log($"Send no queue message to cloud: {JsonConvert.SerializeObject(message)}");
//                return true;
//            }
//            catch (Exception e)
//            {
//                 _logger.Error("SendMsgToCloud",e);
//                return false;
//            }
           
//        }

//        //private void InitRabbitMqConnection()
//        //{
//        //    try
//        //    {
//        //        var hostName = _configurationRoot["RabbitMQ:HostName"];
//        //        var userName = _configurationRoot["RabbitMQ:UserName"];
//        //        var pwd = _configurationRoot["RabbitMQ:Password"];

//        //        _detailLogService.Log($"Connecting to RabbitMQ {hostName}");
//        //        var factory = new ConnectionFactory() { HostName = hostName, UserName = userName, Password = pwd };

//        //        _connection = factory.CreateConnection(_machineId);
//        //        _queuedChannel = _connection.CreateModel();
//        //        _noQueueChannel = _connection.CreateModel();

//        //    }
//        //    catch (Exception e)
//        //    {
//        //       _logger.Error("InitRabbitMqConnection",e);
//        //    }
            


//        //}

//        //private void CheckConnectTimerElapsed(object sender, ElapsedEventArgs e)
//        //{
//        //    if (!_connection.IsOpen)
//        //    {
//        //        _detailLogService.Log("RabbitMQ connection closed, try to reopen...");
//        //        if (_queuedChannel.IsOpen) _queuedChannel.Close();

//        //        InitRabbitMqConnection();
//        //    }
//        //}

//        //public void Dispose()
//        //{
//        //    _detailLogService.Log("RabbitMQ disposed");
//        //    _connection?.Dispose();
//        //    _queuedChannel?.Dispose();
//        //    _noQueueChannel?.Dispose();
//        //    _checkConnectTimer?.Dispose();
//        //}
//    }
//}
