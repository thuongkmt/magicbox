using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KonbiBrain.Common.Services;
using KonbiBrain.Messages;
using MdbCashlessBrain;
using Microsoft.Extensions.Configuration;
using NsqSharp;

namespace MdbCashlessConsoleBrain
{
    class Program
    {
        public const string ServiceName = "MdbCashlessService";
        static void Main(string[] args)
        {
                
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            //Console.WriteLine(configuration.GetConnectionString("Storage"));

            var machineRestSvc=new MachineAdminRestService(configuration);
            var task = machineRestSvc.GetPort();
            task.Wait();
            var selectedPort = task.Result;

            var mdbProcessingService = new MdbProcessingService();

            var consumer = new Consumer(NsqTopics.PAYMENT_REQUEST_TOPIC, NsqConstants.NsqDefaultChannel);
            consumer.AddHandler(new PaymentRequestMessageHandler(mdbProcessingService));
            Console.WriteLine(NsqConstants.NsqUrlConsumer);
            consumer.ConnectToNsqLookupd(NsqConstants.NsqUrlConsumer);
            mdbProcessingService.NsqMessageProducerService = new NsqMessageProducerService();
            mdbProcessingService.LogService=new LogService();
            Start(mdbProcessingService,selectedPort);

            Console.WriteLine("Mdb Started!");
            Console.ReadLine();
            

        }


        private static void Start(MdbProcessingService svc,string port)
        {
            try
            {
                svc.SetPort(port);
                svc.StartBackgroundWorker();
                Thread.Sleep(3000);
                Task.Factory.StartNew(() => svc.InitMdb());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}
