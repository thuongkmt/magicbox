//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Runtime.InteropServices;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using StompSharp.Messages;

//namespace StompSharp
//{
//    /*
//    public class StompClient 
//    {
//        private readonly TcpClient _client;
//        private readonly object _syncRoot = new object();
//        private readonly StreamWriter _writer;
//        private readonly IMessageSerializer _messageSerializer;

//        private readonly Subject<IMessage> _incommingMessages = new Subject<IMessage>();
//        private bool _disposed;


//        public StompClient(string address, int port)
//        {
//            _client = new TcpClient();
//            _client.Connect(address, port);

//            _writer = new StreamWriter(_client.GetStream(), Encoding.ASCII);
//            _messageSerializer = new StreamMessageSerializer();
//        }

//        private void WriteMessageSync(IMessage message)
//        {
//            lock (_syncRoot)
//            {
//                _messageSerializer.Serialize(message, _writer);
//            }
//        }

//        private void ReadMessages()
//        {
//            var reader = new StreamReader(_client.GetStream(), Encoding.ASCII);
//            var messageFactory = new StreamMessageFactory();

//            while (!_disposed)
//            {
//                _incommingMessages.OnNext(messageFactory.Create(reader));
//            }
//        }

//        public ITransport Transport { get; private set; }

//        public IDestination GetDestination(string destination)
//        {
//            throw new NotImplementedException();
//        }

//        public IObservable<IMessage> Subscribe(string destination)
//        {
//            // TODO : Count check... :O

//            return Observable.Create<IMessage>(o =>
//            {
//                // Register incomming messages with specific destination
//                _incommingMessages.Where(
//                    i =>
//                        i.Command == "MESSAGE" &&
//                        i.Headers.FirstOrDefault(h => h.Key == "destination" && h.Value.ToString() == destination) !=
//                        null).
//                    Subscribe(o);

//                WriteMessageSync(new MessageBuilder("SUBSCRIBE").Header("destination", destination).WithoutBody());

//                // Unsubscribe :)
//                return Disposable.Create(() => WriteMessageSync(new MessageBuilder("UNSUBSCRIBE").Header("destination", destination).WithoutBody()));
//            });
//        }

//        public void Dispose()
//        {
//            _disposed = true;
//        }
//    }*/


//    class Program
//    {
//        static void Main(string[] args)
//        {
//            /*
//            // Stomp connection
//            TcpClient client = new TcpClient();

//            client.Connect("localhost", 61613);

//            Task.Factory.StartNew(() =>
//            {
//                var reader = new StreamReader(client.GetStream(), Encoding.ASCII);
//                var messageFactory = new StreamMessageFactory(reader);
//                while (true)
//                {
//                    Console.WriteLine(messageFactory.Create());
//                }
//            });

//            var writer = new StreamWriter(client.GetStream(), Encoding.ASCII);
//            var messageSerializer = new StreamMessageSerializer(writer);

//            messageSerializer.Serialize(new MessageBuilder("CONNECT").Header("accept-version", "1.2").WithoutBody()).Wait();

//            messageSerializer.Serialize(new MessageBuilder("SUBSCRIBE").Header("id", 1).Header("destination", "/queue/a").WithoutBody()).Wait();

//            for (int i = 0; i < 1000; i++)
//            {
//                messageSerializer.Serialize(
//                new MessageBuilder("SEND").Header("destination", "/queue/a")
//                    .Header("receipt", "message-" + i)
//                    .Header("inner-id", i)
//                    .WithBody(Encoding.UTF8.GetBytes("Hello World"))).Wait();

//            }
//            */

//            StompClient client = new StompClient("localhost", 61613);

//            //client.Transport.IncommingMessages.GetObservable("ERROR").Subscribe(m => Console.WriteLine("ERROR!" + m));
//            var destination = client.GetDestination("/topic/demo", client.SubscriptionBehaviors.AutoAcknowledge);

//            IReceiptBehavior receiptBehavior =
//                new ReceiptBehavior(destination.Destination, client.Transport.IncommingMessages);
//            receiptBehavior = NoReceiptBehavior.Default;
//            //Stopwatch sw = Stopwatch.StartNew();

//            var task = client.BeginTransaction();
//            task.Wait();
//            var transaction = task.Result;
//            SendMessages(destination, receiptBehavior, transaction).Wait();
//            //transaction.Commit();

//            //Console.WriteLine("Subscribed to messages, Press enter to ack last one");
//            //Console.ReadLine();

//            ////using (var transaction = client.BeginTransaction()) { 
//            //using (destination.IncommingMessages.Subscribe(WriteMessageId))
//            //{
//            //    //var task = client.BeginTransaction();
//            //    //task.Wait();
//            //    //var transaction = task.Result;
//            //    //SendMessages(destination, receiptBehavior, transaction).Wait();
//            //    //transaction.Commit();

//            //    Console.WriteLine("Subscribed to messages, Press enter to ack last one");
//            //    Console.ReadLine();

//            //    //lastMessage.Ack().Wait();
//            //    Console.WriteLine("Ackd last message, press enter to end subscription and dispose client");
//            //    Console.ReadLine();
//            //}

//            //}

//            client.Dispose();
//            //Console.WriteLine(sw.Elapsed);
        
//            Console.WriteLine("Sending...");
//            Console.ReadLine();
//        }

//        private static int messageSeq;

//        private static readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);

//        private static IMessage lastMessage;

//        private static void WriteMessageId(IMessage obj)
//        {
            
//            messageSeq++;

//            //if (messageSeq == 0)
//            {
//                Console.WriteLine("Read : " + messageSeq+" "+obj.Command);
//            }
//        }

//        private static void WriteMessageAsFile(IMessage obj)
//        {
//            string tempFileName = Path.GetTempFileName();
//            File.WriteAllBytes(tempFileName, obj.Body);

//            Console.WriteLine(tempFileName);
//        }

//        private static async Task SendMessages(IDestination destination, IReceiptBehavior receiptBehavior, IStompTransaction transaction)
//        {
//            var bodyOutgoingMessage =
//                new BodyOutgoingMessage(Encoding.ASCII.GetBytes("Hello")).WithPersistence(); //.WithTransaction(transaction);

//            for (int i = 0; i < 10000; i++)
//            {
//                Console.WriteLine("sending");

//                await destination.SendAsync(bodyOutgoingMessage, receiptBehavior);
//            }

//            await transaction.Commit();

//            Console.WriteLine("Done sending");

//        }
//    }
//}
