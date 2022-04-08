using System;
using Konbini.Messages.Enums;
using MessagePack;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Konbini.Messages.Services
{
    public class RabbitMqSendMessageToMachineService : ISendMessageToMachineClientService
    {
        //private IConnection _connection;
        //private IModel _queuedChannel;
        //private IModel _noQueueChannel;
        //private ILogger _logger;

        //private readonly IConfigurationRoot _configurationRoot;
        //private readonly IDetailLogService _detailLogService;

        private readonly IConnectToRabbitMqMessageService _connectToRabbitMqService;
        private bool isConnectedToServer = false;
        public Action<Exception,string> ErrorAction { get; set; }

        private readonly string _machineId;
        public RabbitMqSendMessageToMachineService(IConnectToRabbitMqMessageService connectToRabbitMqService)
        {

            _connectToRabbitMqService = connectToRabbitMqService;
        }

        public void InitConfigAndConnect(string hostName, string uesrName, string pwd)
        {
            _connectToRabbitMqService.Connect(hostName, uesrName, pwd);
            isConnectedToServer = true;
        }

        public bool SendQueuedMsgToMachines(KeyValueMessage message, CloudToMachineType machineType)
        {
            if (!isConnectedToServer)
                throw new InvalidOperationException("Please call InitConfigAndConnect medthod first!");
            try
            {
                var _queuedChannel = _connectToRabbitMqService.GetQueuedModel();
                _queuedChannel.ExchangeDeclare(exchange: RabbitMqConstants.EXCHANGE_CLOUD_TO_MACHINE_QUEUED, type: "direct");

                var body = MessagePackSerializer.Serialize(message);

                var properties = _queuedChannel.CreateBasicProperties();
                properties.Persistent = true;

                switch (machineType)
                {
                    case CloudToMachineType.AllMachines:
                        _queuedChannel.BasicPublish(exchange: RabbitMqConstants.EXCHANGE_CLOUD_TO_MACHINE_QUEUED,
                               routingKey: RabbitMqConstants.ROUTING_KEY_MACHINES,
                               basicProperties: properties,
                               body: body);
                        break;
                    case CloudToMachineType.CurrentTenant:
                        break;
                    case CloudToMachineType.ToMachineId:
                        _queuedChannel.BasicPublish(exchange: "",
                              routingKey: message.MachineId.ToString(),
                              basicProperties: properties,
                              body: body);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(machineType), machineType, null);
                }

                MessageLogUtil.Log($"Send message type {machineType.ToString()} to machines: {JsonConvert.SerializeObject(message)}");
                return true;

            }
            catch (Exception e)
            {
                MessageLogUtil.Error("SendQueuedMsgToMachines", e);
                ErrorAction?.Invoke(e, "SendQueuedMsgToMachines");
                return false;
            }


        }

        public bool SendMsgToCloud(KeyValueMessage message, CloudToMachineType machineType)
        {
            if (!isConnectedToServer)
                throw new InvalidOperationException("Please call InitConfigAndConnect medthod first!");
            try
            {
                var _noQueueChannel = _connectToRabbitMqService.GetNoQueuedModel();
                _noQueueChannel.ExchangeDeclare(RabbitMqConstants.EXCHANGE_M2CLOUD_NOQUEUE, "fanout");
                var body = MessagePackSerializer.Serialize(message);

                _noQueueChannel.BasicPublish(exchange: RabbitMqConstants.EXCHANGE_M2CLOUD_NOQUEUE,
                    routingKey: "",
                    basicProperties: null,
                    body: body);

                MessageLogUtil.Log($"Send no queue message to cloud: {JsonConvert.SerializeObject(message)}");
                return true;
            }
            catch (Exception e)
            {
                MessageLogUtil.Error("SendMsgToCloud", e);
                ErrorAction?.Invoke(e, "SendMsgToCloud");
                return false;
            }

        }

        public bool SendQueuedMsgToMachinesLinePay(KeyValueMessage message, CloudToMachineType machineType)
        {
            if (!isConnectedToServer)
                throw new InvalidOperationException("Please call InitConfigAndConnect medthod first!");
            try
            {
                var _queuedChannel = _connectToRabbitMqService.GetQueuedModel();
                _queuedChannel.ExchangeDeclare(exchange: RabbitMqConstants.EXCHANGE_CLOUD_TO_MACHINE_QUEUED_LINE, type: "direct");

                var body = MessagePackSerializer.Serialize(message);

                var properties = _queuedChannel.CreateBasicProperties();
                properties.Persistent = true;

                switch (machineType)
                {
                    case CloudToMachineType.AllMachines:
                        _queuedChannel.BasicPublish(exchange: RabbitMqConstants.EXCHANGE_CLOUD_TO_MACHINE_QUEUED_LINE,
                               routingKey: RabbitMqConstants.ROUTING_KEY_MACHINES,
                               basicProperties: properties,
                               body: body);
                        break;
                    case CloudToMachineType.CurrentTenant:
                        break;
                    case CloudToMachineType.ToMachineId:
                        _queuedChannel.BasicPublish(exchange: "",
                              routingKey: message.MachineId.ToString(),
                              basicProperties: properties,
                              body: body);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(machineType), machineType, null);
                }

                MessageLogUtil.Log($"Send message type {machineType.ToString()} to machines: {JsonConvert.SerializeObject(message)}");
                return true;

            }
            catch (Exception e)
            {
                MessageLogUtil.Error("SendQueuedMsgToMachines", e);
                ErrorAction?.Invoke(e, "SendQueuedMsgToMachines");
                return false;
            }
        }
    }
}
