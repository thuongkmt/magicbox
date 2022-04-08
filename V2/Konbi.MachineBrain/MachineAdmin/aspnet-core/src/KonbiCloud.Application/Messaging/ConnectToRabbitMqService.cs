//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Timers;
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
//    public class ConnectToRabbitMqService:IConnectToRabbitMqService
//    {
//        private IConnection _connection;
//        private IModel _queuedChannel;
//        private IModel _noQueueChannel;
//        private ILogger _logger;

//        private readonly IConfigurationRoot _configurationRoot;
//        private readonly IDetailLogService _detailLogService;
//        private readonly Timer _checkConnectTimer;
//        private readonly string _machineId;

//        public ConnectToRabbitMqService(IAppConfigurationAccessor configurationRoot,
//            IDetailLogService detailLogService, ILogger logger, ISettingManager settingManager)
//        {
//            _machineId = settingManager.GetSettingValue(AppSettingNames.MachineId);
//            _configurationRoot = configurationRoot.Configuration;
//            _detailLogService = detailLogService;
//            _logger = logger;


//            //Connect();

//            _checkConnectTimer = new Timer(60000);//timer run every minutes to check connection
//            _checkConnectTimer.Elapsed += CheckConnectTimerElapsed;
//            _checkConnectTimer.Enabled = true;
//            _checkConnectTimer.Start();


//        }

    

//        public void Connect()
//        {
//            try
//            {
//                var hostName = _configurationRoot["RabbitMQ:HostName"];
//                var userName = _configurationRoot["RabbitMQ:UserName"];
//                var pwd = _configurationRoot["RabbitMQ:Password"];

//                _detailLogService.Log($"Connecting to RabbitMQ {hostName}");
//                var factory = new ConnectionFactory() { HostName = hostName, UserName = userName, Password = pwd };

//                _connection = factory.CreateConnection(_machineId);
//                _queuedChannel = _connection.CreateModel();
//                _noQueueChannel = _connection.CreateModel();

//            }
//            catch (Exception e)
//            {
//                _logger.Error("InitRabbitMqConnection", e);
//            }

//        }

//        public IModel GetQueuedModel()
//        {
//            return _queuedChannel;
//        }

//        public IModel GetNoQueuedModel()
//        {
//            return _noQueueChannel;
//        }

//        public IConnection GetConnection()
//        {
//            return _connection;
//        }


//        private void CheckConnectTimerElapsed(object sender, ElapsedEventArgs e)
//        {
//            if (!_connection.IsOpen)
//            {
//                _detailLogService.Log("RabbitMQ connection closed, try to reopen...");
//                if (_queuedChannel.IsOpen) _queuedChannel.Close();

//                Connect();
//            }
//        }

//        public void Dispose()
//        {
//            _detailLogService.Log("RabbitMQ disposed");
//            _connection?.Dispose();
//            _queuedChannel?.Dispose();
//            _noQueueChannel?.Dispose();
//            _checkConnectTimer?.Dispose();
//        }

   
//    }
//}
