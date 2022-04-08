using System;
using RabbitMQ.Client;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;

namespace Konbini.RfidFridge.RabitMq.Test.Send
{
    class Program
    {
        static void Main(string[] args)
        {

            // HelloWorld();

            //WorksQueue(args);
            //Console.ReadLine();
            // Topic(args);


            //var rpcClient = new RpcClient();

            //Console.WriteLine(" [x] Requesting fib(30)");
            //var response = rpcClient.Call("30");

            //Console.WriteLine(" [.] Got '{0}'", response);
            //rpcClient.Close();

            Topic(args);
            Console.ReadLine();
        }

        public static void Topic(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                channel.ExchangeDeclare(exchange: "test_topic", type: "topic");
                channel.QueueDeclare(queue: "manhsida", durable: true, exclusive: false, autoDelete: false, arguments: null);
                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;
                var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";
                var message = (args.Length > 1)
                                  ? string.Join(" ", args.Skip(1).ToArray())
                                  : "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "test_topic", routingKey: routingKey, basicProperties: properties, body: body);
                Console.WriteLine(" [x] Sent '{0}':'{1}'", routingKey, message);
            }
        }

        public static void Routing(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "tagid", type: "fanout");

                var severity = (args.Length > 0) ? args[0] : "info";
                var message = (args.Length > 1) ? string.Join(" ", args.Skip(1).ToArray()) : "Hello World!";


                var products = new List<Product>();

                int index = 1;
                while (true)
                {

                    message = Console.ReadLine();
                    products.Add(new Product() { Name = $"Product {index}", TagId = message, Price = index * 10 });

                    var json = JsonConvert.SerializeObject(products);

                    var body = Encoding.UTF8.GetBytes(json);





                    channel.BasicPublish(exchange: "tagid", routingKey: severity, basicProperties: null, body: body);
                    Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
                }

            }


        }

        public static void WorksQueue(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue1", durable: true, exclusive: false, autoDelete: false, arguments: null);

                while (true)
                {
                    var message = Console.ReadLine();
                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(exchange: "", routingKey: "task_queue1", basicProperties: properties, body: body);
                    Console.WriteLine(" [x] Sent {0}", message);
                }
            }


        }

        public static void HelloWorld()
        {
            #region Hello World
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

                while (true)
                {
                    var data = Console.ReadLine();

                    var body = Encoding.UTF8.GetBytes(data);

                    channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
                    Console.WriteLine(" [>] Sent {0}", data);
                }
            }
            #endregion

        }


        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
        }
    }


    public class RpcClient
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;

        public RpcClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var response = Encoding.UTF8.GetString(body);
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        respQueue.Add(response);
                    }
                };
        }

        public string Call(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(
                exchange: "",
                routingKey: "rpc_queue",
                basicProperties: props,
                body: messageBytes);

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: true);

            return respQueue.Take(); ;
        }

        public void Close()
        {
            connection.Close();
        }
    }

    public class Product
    {
        public string Name;
        public string TagId;
        public decimal Price;
    }
}
