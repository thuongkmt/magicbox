using LinePayCSharp;
using MessagePack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LinePay
{
    public class ConsumeRabbitMQHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private IConfiguration configuration { get; set; }

        public ConsumeRabbitMQHostedService(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<ConsumeRabbitMQHostedService>();
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            configuration = builder.Build();
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            var hostName = configuration["RabbitMQ:HostName"];
            var userName = configuration["RabbitMQ:UserName"];
            var pwd = configuration["RabbitMQ:Password"];
            var clientName = "LinePay-Machine:" + configuration["LinePay:MachineId"];

            var factory = new ConnectionFactory { HostName = hostName, UserName = userName, Password = pwd };

            // create connection  
            _connection = factory.CreateConnection(clientName);

            // create channel  
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("KonbiCloud2MachineExchangeQueuedLinePay", ExchangeType.Direct);
            _channel.QueueDeclare(queue: configuration["LinePay:MachineId"],
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            _channel.QueueBind(configuration["LinePay:MachineId"], "KonbiCloud2MachineExchangeQueuedLinePay", "KonbiAllMachinesRoutingKey");
            _channel.QueueBind(configuration["LinePay:MachineId"], "KonbiCloud2MachineExchangeQueuedLinePay", configuration["LinePay:MachineId"]);
            _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var body = ea.Body;
                var message = MessagePackSerializer.Deserialize<KeyValueMessage>(body);

                // received message  
                //var content = System.Text.Encoding.UTF8.GetString(ea.Body);

                // handle the received message  
                HandleMessage(message);
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(configuration["LinePay:MachineId"], false, consumer);
            return Task.CompletedTask;
        }

        private void HandleMessage(KeyValueMessage keyValueMessage)
        {
            // we just print this message   
            //_logger.LogInformation($"consumer received {content}");
            var key = keyValueMessage.Key;
            switch (key)
            {
                case MessageKeys.LinePayCloseLock:
                    {
                        try
                        {
                            var linePay = JsonConvert.DeserializeObject<LinePayFinishDto>(keyValueMessage.JsonValue);

                            var reserve = CacheService.Cache[linePay.transactionId] as Reserve;
                            reserve.Capture = true;
                            reserve.ProductName = !String.IsNullOrEmpty(linePay.productName) ? linePay.productName : "Empty" ;
                            reserve.Amount = linePay.amount;
                        }
                        catch (Exception ex)
                        {
                            break;
                        }
                        
                       
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }

    public class LinePayFinishDto
    {
        public string regkey { get; set; }
        public Int64 transactionId { get; set; }
        public int amount { get; set; }
        public string productName { get; set; }
    }
}
