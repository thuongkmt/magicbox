using System;
using Abp.Dependency;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiCloud.Common;
using Konbini.Messages;
using MessagePack;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using KonbiCloud.Configuration;
using Konbini.Messages.Enums;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Abp.Configuration;
using KonbiCloud.Messaging;
using Konbini.Messages.Services;

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
        private readonly IProductTagsMessageHandler _productTagsMessageHandler;
        private readonly string _machineId;
        private readonly string _machineName;

        private readonly IConfigurationRoot _configurationRoot;
        private readonly IProductCategoryMessageHandler _productCategoryMessageHandler;
        private readonly IProductCategoryRelationMessageHandler _productCategoryRelationMessageHandler;
        private readonly IProductMachinePriceMessageHandler _productMachinePriceMessageHandler;
        private readonly IManuallySyncProductsMessageHandler _manuallySyncProductsMessageHandler;
        private readonly IManuallySyncProductCategoriesMessageHandler _manuallySyncProductCategoriesMessageHandler;
        private readonly IAlertConfigurationsMessageHandler _alertConfigurationsMessageHandler;
        private readonly ISettingManager _settingManager;
        private readonly ILinePayMessageHandler _linePayMessageHandler;
        private readonly ISyncInventoryToCloudMessageHandler _syncInventoryToCloudMessageHandler;

        public RabbitMqListenerJob(AbpTimer timer,
            IDetailLogService logService,
            IConnectToRabbitMqMessageService connectToRabbitMqService,
            ISettingManager settingManager,
            IProductMessageHandler productMessageHandler,
            IAppConfigurationAccessor configurationRoot,
            IProductTagsMessageHandler productTagsMessageHandler,
            IProductCategoryMessageHandler productCategoryMessageHandler,
            IProductCategoryRelationMessageHandler productCategoryRelationMessageHandler,
            IProductMachinePriceMessageHandler productMachinePriceMessageHandler,
            IManuallySyncProductsMessageHandler manuallySyncProductsMessageHandler,
            IManuallySyncProductCategoriesMessageHandler manuallySyncProductCategoriesMessageHandler,
            IAlertConfigurationsMessageHandler alertConfigurationsMessageHandler,
            ILinePayMessageHandler linePayMessageHandler,
            ISyncInventoryToCloudMessageHandler syncInventoryToCloudMessageHandler
            ) : base(timer)
        {
            Timer.Period = 60000; //check connection every 1 minutes!
            Timer.RunOnStart = true;

            _machineId = settingManager.GetSettingValue(AppSettingNames.MachineId);
            _machineName = settingManager.GetSettingValue(AppSettingNames.MachineName);

            _connectToRabbitMqService = connectToRabbitMqService;
            _productMessageHandler = productMessageHandler;
            this.detailLogService = logService;
            _configurationRoot = configurationRoot.Configuration;
            _productTagsMessageHandler = productTagsMessageHandler;
            _productCategoryMessageHandler = productCategoryMessageHandler;
            _productCategoryRelationMessageHandler = productCategoryRelationMessageHandler;
            _productMachinePriceMessageHandler = productMachinePriceMessageHandler;
            _manuallySyncProductsMessageHandler = manuallySyncProductsMessageHandler;
            _manuallySyncProductCategoriesMessageHandler = manuallySyncProductCategoriesMessageHandler;
            _alertConfigurationsMessageHandler = alertConfigurationsMessageHandler;
            _settingManager = settingManager;
            _linePayMessageHandler = linePayMessageHandler;
            _syncInventoryToCloudMessageHandler = syncInventoryToCloudMessageHandler;

            StartJob();
        }
        protected override void DoWork()
        {
            if (_connectToRabbitMqService.GetConnection() != null)
            {
                if (!_connectToRabbitMqService.GetConnection().IsOpen)
                {
                    var cloudChannel = _connectToRabbitMqService.GetQueuedModel();
                    var cloudNoQueueChannel = _connectToRabbitMqService.GetNoQueuedModel();
                    if (cloudChannel.IsOpen) cloudChannel.Close();
                    if (cloudNoQueueChannel.IsOpen) cloudNoQueueChannel.Close();
                    //reconnect rabbitmq
                    StartJob();
                }
            }
            else
            {
                detailLogService.Log($"DoWork() | _connectToRabbitMqService.GetConnection() is null");
            }

        }


        public void StartJob()
        {
            try
            {
                var hostName = _settingManager.GetSettingValue(AppSettingNames.RabbitMqServer);
                var userName = _settingManager.GetSettingValue(AppSettingNames.RabbitMqUser);
                var pwd = _settingManager.GetSettingValue(AppSettingNames.RabbitMqPassword);

                var machineName = _machineName?.Replace(" ", string.Empty);
                var clientName = $"ListenerJob-{machineName}-{_machineId}";
                _connectToRabbitMqService.Connect(hostName, userName, pwd,clientName);

                //ConsumeClientNoQueuedMessages();
                ConsumeClientQueuedMessages();
            }
            catch (Exception ex)
            {
                Logger.Error("StartRabbitMQ error", ex);
            }

        }

        private void ConsumeClientQueuedMessages()
        {

            var cloudToMachineChannel = _connectToRabbitMqService.GetQueuedModel();
            detailLogService.Log($"RabbitMQ queued established connection");

            if (cloudToMachineChannel == null)
            {
                detailLogService.Log($"cloudToMachineChannel is null, there is problem with connection!");
                return;
            }
            cloudToMachineChannel.ExchangeDeclare(RabbitMqConstants.EXCHANGE_CLOUD_TO_MACHINE_QUEUED, "direct");

            //machine stored in machine id queue
            cloudToMachineChannel.QueueDeclare(queue: _machineId,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            ;

            //declare routing keys: current support: all machines, tenant, machine id
            cloudToMachineChannel.QueueBind(_machineId, RabbitMqConstants.EXCHANGE_CLOUD_TO_MACHINE_QUEUED, RabbitMqConstants.ROUTING_KEY_MACHINES);
            cloudToMachineChannel.QueueBind(_machineId, RabbitMqConstants.EXCHANGE_CLOUD_TO_MACHINE_QUEUED, _machineId);
            //TODO: tenant binding


            _queuedConsumer = new EventingBasicConsumer(cloudToMachineChannel);
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
                cloudToMachineChannel.BasicAck(ea.DeliveryTag, false);
            };

            cloudToMachineChannel.BasicConsume(queue: _machineId,
                autoAck: false,
                consumer: _queuedConsumer);
        }


        private void ConsumeClientNoQueuedMessages()
        {
            var _clientToCloudNoQueueChannel = _connectToRabbitMqService.GetNoQueuedModel();

            _clientToCloudNoQueueChannel.ExchangeDeclare(RabbitMqConstants.EXCHANGE_M2CLOUD_NOQUEUE, "fanout");
            detailLogService.Log($"RabbitMQ no-queue established connection");
            var queueName = _clientToCloudNoQueueChannel.QueueDeclare().QueueName;

            _clientToCloudNoQueueChannel.QueueBind(queueName, RabbitMqConstants.EXCHANGE_M2CLOUD_NOQUEUE, "");

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
                    case MessageKeys.LinePayOpenLock:
                        return await _linePayMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.Transaction:
                        //await ProcessTransaction(keyValueMessage);
                        break;
                    case MessageKeys.Product:
                        //await ProcessProduct(keyValueMessage);
                        return await _productMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.Inventory:
                        //await ProcessInventory(keyValueMessage);
                        break;
                    case MessageKeys.Topup:
                        //await ProcessTopup(keyValueMessage);
                        break;
                    case MessageKeys.UpdateInventoryList:
                        //await ProcessUpdateInventoryList(keyValueMessage);
                        break;
                    case MessageKeys.ProductRfidTags:
                        return await _productTagsMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.ProductCategory:
                        return await _productCategoryMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.ProductCategoryRelations:
                        return await _productCategoryRelationMessageHandler.Handle(keyValueMessage);
                    // Product machine price.
                    case MessageKeys.ProductMachinePrice:
                        // Save price for products.
                        return await _productMachinePriceMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.ManuallySyncProduct:
                        return await _manuallySyncProductsMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.ManuallySyncProductCategory:
                        return await _manuallySyncProductCategoriesMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.AlertConfiguration:
                        return await _alertConfigurationsMessageHandler.Handle(keyValueMessage);
                    case MessageKeys.SyncInventoriesToCloud:
                        return await _syncInventoryToCloudMessageHandler.Handle(keyValueMessage);
                    default:
                        break;
                }
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("ProcessIncomingMessage", e);
                return false;
            }

        }


    }
}
