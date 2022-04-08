using System;
using Konbini.Messages;
using MessagePack;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Konbini.Messages.Services
{
    public class RabbitMqSendMessageToCloudService : ISendMessageToCloudService
    {

        //private string _machineId;
        private readonly IConnectToRabbitMqMessageService _connectToRabbitMqService;
        private bool isConnectedToServer = false;
        public Action<Exception, string> ErrorAction { get; set; }

        public void InitConfigAndConnect(string hostName, string uesrName, string pwd, string clientName)
        {
            _connectToRabbitMqService.Connect(hostName, uesrName, pwd, clientName);
            //_machineId = machineId.ToString();
            isConnectedToServer = true;
        }


        public RabbitMqSendMessageToCloudService(IConnectToRabbitMqMessageService connectToRabbitMqService)
        {
            _connectToRabbitMqService = connectToRabbitMqService;
        }

        public bool SendQueuedMsgToCloud(KeyValueMessage message)
        {
            if (!isConnectedToServer)
                throw new InvalidOperationException("Please call InitConfigAndConnect medthod first!");
            try
            {
                var _queuedChannel = _connectToRabbitMqService.GetQueuedModel();
                _queuedChannel.QueueDeclare(queue: RabbitMqConstants.CLIENT_TO_SERVER_QUEUE,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var body = MessagePackSerializer.Serialize(message);

                var properties = _queuedChannel.CreateBasicProperties();
                properties.Persistent = true;

                _queuedChannel.BasicPublish(exchange: "",
                    routingKey: RabbitMqConstants.CLIENT_TO_SERVER_QUEUE,
                    basicProperties: properties,
                    body: body);
                MessageLogUtil.Log($"Send message to cloud: {JsonConvert.SerializeObject(message)}");
                return true;

            }
            catch (Exception e)
            {
                _connectToRabbitMqService.Reconnect();
                MessageLogUtil.Error("SendQueuedMsgToCloud", e);
                ErrorAction?.Invoke(e, "SendQueuedMsgToCloud");
                return false;
            }


        }

        public bool SendMsgToCloud(KeyValueMessage message)
        {
            MessageLogUtil.Log($"Send no queue message to cloud: {JsonConvert.SerializeObject(message)}");

            if (!isConnectedToServer)
            {
                throw new InvalidOperationException("Please call InitConfigAndConnect method first!");
            }
            try
            {
                var _noQueueChannel = _connectToRabbitMqService.GetNoQueuedModel();

                var queueName = $"{message.Key}-{message.MachineId}-{_connectToRabbitMqService.GetIdentity()}";

                //if(_noQueueChannel.CloseReason != null)
                //{
                //    _noQueueChannel.QueueDelete(queueName);
                //}

                _noQueueChannel.QueueDeclare(queue: queueName, durable: false, autoDelete: true);
                _noQueueChannel.ExchangeDeclare(RabbitMqConstants.EXCHANGE_M2CLOUD_NOQUEUE, "fanout");
                var body = MessagePackSerializer.Serialize(message);

                //IBasicProperties props = _noQueueChannel.CreateBasicProperties();
                //props.ContentType = "text/plain";
                //props.DeliveryMode = 2;
                //props.Expiration = "3000";

                _noQueueChannel.BasicPublish(exchange: RabbitMqConstants.EXCHANGE_M2CLOUD_NOQUEUE,
                    routingKey: "",
                    basicProperties: null,
                    body: body);

                return true;
            }
            catch (Exception e)
            {
                _connectToRabbitMqService.Reconnect();
                MessageLogUtil.Log(e.ToString());
                //MessageLogUtil.Error("SendMsgToCloud", e);
                ErrorAction?.Invoke(e, "SendMsgToCloud");
                return false;
            }

        }

    }
}
