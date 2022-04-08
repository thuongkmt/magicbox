using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace Konbini.RfidFridge.RabitMq.Test.Receive
{
    using RabbitMQ.Client.MessagePatterns;

    class Program
    {
        static void Main(string[] args)
        {
            //HelloWorld();
            //WorkQueue();
            //Rounting(args);
            Topic(args);
            //SubTopicExchange();
           // SimpleRpcServer();
        }


        public static void SimpleRpcServer()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "rpc_queue", durable: false,
                    exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(0, 1, false);
                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: "rpc_queue",
                    autoAck: false, consumer: consumer);
                Console.WriteLine(" [x] Awaiting RPC requests");

                consumer.Received += (model, ea) =>
                    {
                        string response = null;

                        var body = ea.Body;
                        var props = ea.BasicProperties;
                        var replyProps = channel.CreateBasicProperties();
                        replyProps.CorrelationId = props.CorrelationId;

                        try
                        {
                            var message = Encoding.UTF8.GetString(body);
                            int n = int.Parse(message);
                            Console.WriteLine(" [.] fib({0})", message);
                            response = fib(n).ToString();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(" [.] " + e.Message);
                            response = "";
                        }
                        finally
                        {
                            var responseBytes = Encoding.UTF8.GetBytes(response);
                            channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                                basicProperties: replyProps, body: responseBytes);
                            channel.BasicAck(deliveryTag: ea.DeliveryTag,
                                multiple: false);
                        }
                    };

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private static int fib(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }

            return fib(n - 1) + fib(n - 2);
        }

        private static void SubTopicExchange()
        {
            var exchangeName = "test_topic";
            var topic = "#";

            var factory = new ConnectionFactory() { HostName = "128.199.209.115", UserName = "admin", Password = "konbini62" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchangeName, type: "topic");
                var queueName = channel.QueueDeclare().QueueName;
                channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: topic);
                Console.WriteLine("[*] Waiting for messages 1.");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += Consumer_Received;
                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            }
            Console.ReadLine();

        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;
            Console.WriteLine($" [x] Received '{routingKey}':'{message}'");
            // throw new NotImplementedException();
        }

        public static void Topic(string[] args)
        {
            var exchangeName = "test_topic";
            var topic = "#";

            var factory = new ConnectionFactory() { HostName = "localhost"};

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: exchangeName, type: "topic");
            var queueName = "manhsida";//= channel.QueueDeclare().QueueName;

            channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: topic);
            Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;
            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
            //Console.WriteLine(" Press [enter] to exit.");
            //Console.ReadLine();

        }

        public static void Rounting(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "tagid", type: "fanout");
                var queueName = channel.QueueDeclare().QueueName;

                //if (args.Length < 1)
                //{
                //    Console.Error.WriteLine("Usage: {0} [info] [warning] [error]", Environment.GetCommandLineArgs()[0]);
                //    Console.WriteLine(" Press [enter] to exit.");
                //    Console.ReadLine();
                //    Environment.ExitCode = 1;
                //    return;
                //}

                //foreach (var severity in args)
                //{
                channel.QueueBind(queue: queueName, exchange: "tagid", routingKey: "");
                // }

                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" [x] Received '{0}':'{1}'", routingKey, message);
                };

                channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        public static void WorkQueue()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "task_queue1", durable: true, exclusive: false, autoDelete: false, arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    
                    
                    Console.WriteLine(" [*] Waiting for messages.");

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine(" [x] Received {0}", message);
                        int dots = message.Split('.').Length - 1;
                        Thread.Sleep(dots * 1000);
                        Console.WriteLine(" [x] Done");

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };
                    channel.BasicConsume(queue: "task_queue1", autoAck: false, consumer: consumer);


                }
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();

                //using (var channel = connection.CreateModel())
                //{
                //    channel.QueueDeclare(queue: "task_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

                //    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                //    Console.WriteLine(" [*] Waiting for messages.");

                //    var consumer = new EventingBasicConsumer(channel);
                //    consumer.Received += (model, ea) =>
                //    {
                //        var body = ea.Body;
                //        var message = Encoding.UTF8.GetString(body);
                //        Console.WriteLine(" [x] 1Received {0}", message);

                //        int dots = message.Split('.').Length - 1;
                //        Thread.Sleep(dots * 1000);

                //        Console.WriteLine(" [x] Done");

                //        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                //    };
                //    channel.BasicConsume(queue: "task_queue", autoAck: false, consumer: consumer);
                //}


            }



        }

        public static void HelloWorld()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                    int dots = message.Split('.').Length - 1;
                    Thread.Sleep(dots * 1000);

                    Console.WriteLine(" [x] Done");
                };
                channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
