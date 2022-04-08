using System;
using Abp.Dependency;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiCloud.Common;
using Konbini.Messages;
using MessagePack;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Konbini.Messages.Enums;
using System.Threading.Tasks;
using KonbiCloud.Messaging;
using Konbini.Messages.Services;
using Microsoft.Extensions.Configuration;
using KonbiCloud.Configuration;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;

namespace KonbiCloud.BackgroundJobs
{
    public class RabbitMqListenerJob : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        //private IConnection _connection;
        //private IModel _clientToCloudChannel;
        //private IModel _clientToCloudNoQueueChannel;
        private EventingBasicConsumer _queuedConsumer;
        private readonly IDetailLogService detailLogService;
        private readonly IConnectToRabbitMqMessageService _connectToRabbitMqService;


        private readonly IProductMessageHandler _productMessageHandler;
        private readonly ITopupMessageHandler _topupMessageHandler;
        private readonly ITransactionMessageHandler _transactionMessageHandler;
        private readonly IInventoryMessageHandler _inventoryMessageHandler;
        private readonly IUpdateInventoryListMessageHandler _updateInventoryListMessageHandler;
        private readonly IMachineStatusMessageHandler _machineStatusMessageHandler;
        private readonly ITemperatureLogsMessageHandler _temperatureLogsMessageHandler;
        private readonly IProductTagsMessageHandler _productTagsMessageHandler;
        private readonly IChangeTagStateMessageHandler _changeTagStateMessageHandler;
        private readonly IProductCategoryMessageHandler _productCategoryMessageHandler;
        private readonly IProductTagsRealtimeMessageHandler _productTagsRealtimeMessageHandler;
        private readonly IInventoryRestockMessageHandler _inventoryRestockMessageHandler;
        private readonly IManuallySyncProductsMessageHandler _manuallySyncProductsMessageHandler;
        private readonly IManuallySyncProductCategoriesMessageHandler _manuallySyncProductCategoriesMessageHandler;
        private readonly ISyncInventoriesForActiveTopupMessageHandler _syncInventoriesForActiveTopupMessageHandler;
        private readonly IConfigurationRoot _configurationRoot;


        public RabbitMqListenerJob(AbpTimer timer,
                                    IDetailLogService logService,
                                    IConnectToRabbitMqMessageService connectToRabbitMqService,
                                    IProductMessageHandler productMessageHandler,
                                    ITopupMessageHandler topupMessageHandler,
                                    ITransactionMessageHandler transactionMessageHandler,
                                    IInventoryMessageHandler inventoryMessageHandler,
                                    IUpdateInventoryListMessageHandler updateInventoryListMessageHandler,
                                    IMachineStatusMessageHandler machineStatusMessageHandler,
                                    ITemperatureLogsMessageHandler temperatureLogsMessageHandler,
                                    IProductTagsMessageHandler productTagsMessageHandler,
                                    IChangeTagStateMessageHandler changeTagStateMessageHandler ,
                                    IProductCategoryMessageHandler productCategoryMessageHandler,
                                    IAppConfigurationAccessor configurationRoot,
                                    IProductTagsRealtimeMessageHandler productTagsRealtimeMessageHandler,
                                    IInventoryRestockMessageHandler inventoryRestockMessageHandler,
                                    IManuallySyncProductsMessageHandler manuallySyncProductsMessageHandler,
                                    IManuallySyncProductCategoriesMessageHandler manuallySyncProductCategoriesMessageHandler,
                                    ISyncInventoriesForActiveTopupMessageHandler syncInventoriesForActiveTopupMessageHandler
                                    ) : base(timer)
        {
            Timer.Period = 60000; //check connection every 1 minutes!
            Timer.RunOnStart = true;
            _connectToRabbitMqService = connectToRabbitMqService;
            _connectToRabbitMqService.ErrorAction = ConnectRabbitMqError;
            _productMessageHandler = productMessageHandler;
            _topupMessageHandler = topupMessageHandler;
            _transactionMessageHandler = transactionMessageHandler;
            _inventoryMessageHandler = inventoryMessageHandler;
            _updateInventoryListMessageHandler = updateInventoryListMessageHandler;
            _machineStatusMessageHandler = machineStatusMessageHandler;
            _temperatureLogsMessageHandler = temperatureLogsMessageHandler;
            _productTagsMessageHandler = productTagsMessageHandler;
            _changeTagStateMessageHandler = changeTagStateMessageHandler;
            _productCategoryMessageHandler = productCategoryMessageHandler;
            detailLogService = logService;
            _configurationRoot = configurationRoot.Configuration;
            _productCategoryMessageHandler = productCategoryMessageHandler;
            _productTagsRealtimeMessageHandler = productTagsRealtimeMessageHandler;
            _inventoryRestockMessageHandler = inventoryRestockMessageHandler;
            _manuallySyncProductsMessageHandler = manuallySyncProductsMessageHandler;
            _manuallySyncProductCategoriesMessageHandler = manuallySyncProductCategoriesMessageHandler;
            _syncInventoriesForActiveTopupMessageHandler = syncInventoriesForActiveTopupMessageHandler;
            
            StartJob();
        }

        private void ConnectRabbitMqError(Exception e)
        {
            Logger.Error("ConnectRabbitMqError error", e);
        }
        protected override void DoWork()
        {
            //_productTagsRealtimeMessageHandler.Handle(new KeyValueMessage
            //{
            //    Key = MessageKeys.ProductRfidTagsRealtime,
            //    JsonValue = "TuDN Test"
            //}).Wait();
            if(_connectToRabbitMqService.GetConnection() == null)
            {
                Logger.Error("RabbitMQ connection is null");
                return;
            }
            if (!_connectToRabbitMqService.GetConnection().IsOpen)
            {
                var _clientToCloudChannel = _connectToRabbitMqService.GetQueuedModel();
                var _clientToCloudNoQueueChannel = _connectToRabbitMqService.GetNoQueuedModel();
                if (_clientToCloudChannel.IsOpen) _clientToCloudChannel.Close();
                if (_clientToCloudNoQueueChannel.IsOpen) _clientToCloudNoQueueChannel.Close();
                //reconnect rabbitmq
                StartJob();
            }
        }


        public void StartJob()
        {
            try
            {
                var hostName = _configurationRoot["RabbitMQ:HostName"];
                var userName = _configurationRoot["RabbitMQ:UserName"];
                var pwd = _configurationRoot["RabbitMQ:Password"];
                _connectToRabbitMqService.Connect(hostName,userName,pwd);

                ConsumeClientNoQueuedMessages();
                ConsumeClientQueuedMessages();
            }
            catch (Exception ex)
            {
                Logger.Error("StartRabbitMQ error", ex);
            }

        }

        private void ConsumeClientQueuedMessages()
        {

            var _clientToCloudChannel = _connectToRabbitMqService.GetQueuedModel();
            detailLogService.Log($"RabbitMQ queued established connection");
            if (_clientToCloudChannel == null)
            {
                detailLogService.Log($"ConsumeClientQueuedMessages() | _clientToCloudChannel is null");
                return;
            }
            _clientToCloudChannel.QueueDeclare(queue: RabbitMqConstants.CLIENT_TO_SERVER_QUEUE,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _clientToCloudChannel.BasicQos(0, 1, false);
            _queuedConsumer = new EventingBasicConsumer(_clientToCloudChannel);
            _queuedConsumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = MessagePackSerializer.Deserialize<KeyValueMessage>(body);
                var json = MessagePackSerializer.ToJson(body);
                detailLogService.Log($"Received RabbitMQ queued data {json}");

                var successProceedTask = ProcessIncomingMessage(message);

                successProceedTask.Wait();
                //var successProceed = successProceedTask.Result;
                //if(successProceed)
                _clientToCloudChannel.BasicAck(ea.DeliveryTag, false);
            };
            _clientToCloudChannel.BasicConsume(queue: RabbitMqConstants.CLIENT_TO_SERVER_QUEUE,
                autoAck: false,
                consumer: _queuedConsumer);
        }


        private void ConsumeClientNoQueuedMessages()
        {
            var _clientToCloudNoQueueChannel = _connectToRabbitMqService.GetNoQueuedModel();

            if(_clientToCloudNoQueueChannel == null)
            {
                detailLogService.Log($"ConsumeClientNoQueuedMessages() | _clientToCloudNoQueueChannel is null");
                return;
            }
            _clientToCloudNoQueueChannel.ExchangeDeclare(RabbitMqConstants.EXCHANGE_M2CLOUD_NOQUEUE,"fanout");
            detailLogService.Log($"RabbitMQ no-queue established connection");
            var queueName = _clientToCloudNoQueueChannel.QueueDeclare().QueueName;

            _clientToCloudNoQueueChannel.QueueBind(queueName,RabbitMqConstants.EXCHANGE_M2CLOUD_NOQUEUE,"");
            
            var consumer = new EventingBasicConsumer(_clientToCloudNoQueueChannel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = MessagePackSerializer.Deserialize<KeyValueMessage>(body);
                var json = MessagePackSerializer.ToJson(body);
                detailLogService.Log($"Received RabbitMQ no-queue data {json}");
                ProcessIncomingMessage(message).Wait();
            };
            _clientToCloudNoQueueChannel.BasicConsume(queue: queueName,
                autoAck: true,
                consumer: consumer);
        }


        private async Task<bool> ProcessIncomingMessage(KeyValueMessage keyValueMessage)
        {
            try
            {
                var key = keyValueMessage.Key;

                switch (key)
                {
                    case MessageKeys.Transaction:
                        return await _transactionMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.Product:
                        return await _productMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.Inventory:
                        return await _inventoryMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.Topup:
                        return await _topupMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.UpdateInventoryList:
                        return await _updateInventoryListMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.MachineStatus:
                        return await _machineStatusMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.TemperatureLogs:
                        return await _temperatureLogsMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.ProductRfidTags:
                        return await _productTagsMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.UpdateTagState:
                        return await _changeTagStateMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.ProductCategory:
                        return await _productCategoryMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.ProductRfidTagsRealtime:
                        return await _productTagsRealtimeMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.Restock:
                        return await _inventoryRestockMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.ManuallySyncProduct:
                        return await _manuallySyncProductsMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.ManuallySyncProductCategory:
                        return await _manuallySyncProductCategoriesMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.SyncInventoriesForActiveTopup:
                        return await _syncInventoriesForActiveTopupMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.SyncInventoriesToCloud:
                        return await _syncInventoriesForActiveTopupMessageHandler.Handle(keyValueMessage);
                    default:
                        break;
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("ProcessIncomingMessage",e);
                return false;
            }
        }
    }
}
