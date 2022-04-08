using Abp.Dependency;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using KonbiCloud.Common;
using KonbiCloud.SignalR;
using Konbini.Messages;
using MessagePack;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.BackgroundJobs
{
    public class RabbitMqListenerJob : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private IConnection connection;
        private IModel channel;
        private readonly IDetailLogService detailLogService;
        private readonly IMessageCommunicator messageCommunicator;

        public RabbitMqListenerJob(AbpTimer timer, IDetailLogService logService, IMessageCommunicator messageCommunicator) : base(timer)
        {
            Timer.Period = 60000; //check connection every 1 minutes!
            this.messageCommunicator = messageCommunicator;
            this.detailLogService = logService;
            StartJob();
        }
        protected override void DoWork()
        {
            if (!connection.IsOpen)
            {
                if (channel.IsOpen) channel.Close();

                //reconnect rabbitmq
                StartJob();
            }
        }


        public void StartJob()
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "128.199.209.115", UserName= "admin", Password= "konbini62" };
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
                detailLogService.Log($"RabbitMQ established connection");
                channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = MessagePackSerializer.Deserialize<KeyValueMessage>(body);
                    var json = MessagePackSerializer.ToJson(body);
                    detailLogService.Log($"Received RabbitMQ data {json}");

                    messageCommunicator.SendMessageToAllClient(new GeneralMessage { Message = json }).Wait();
                    Console.WriteLine(" [x] Received {0}", json);
                };
                channel.BasicConsume(queue: "hello",
                                     autoAck: true,
                                     consumer: consumer);
            }
            catch (Exception ex)
            {
                Logger.Error("StartRabbitMQ error", ex);
            }

        }
    }

}
