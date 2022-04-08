using Konbini.RfidFridge.Service.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.HwController
{
    using Autofac;
    using Common;
    using Domain.DTO;
    using Domain.Enums;
    using Konbini.Messages;
    using Konbini.Messages.Enums;
    using Konbini.Messages.Services;
    using Konbini.RfidFridge.Domain;
    using Konbini.RfidFridge.Service.Data;
    using Konbini.RfidFridge.Service.Devices;
    using Microsoft.Owin.Hosting;
    using Service.Core;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Threading;
    using PaymentType = Domain.Enums.PaymentType;

    public class TestApplication
    {
        private LogService LogService;

        private FridgeInterface FridgeInterface;
        private NayaxInterface NayaxInterface;

        private RabbitMqService RabbitMqService;

        private StompService StompService;

        private WebApiService WebApiService;
        private MachineStatus _machineStatus { get; set; }

        private ITransactionService TransactionService;

        private SlackService SlackService;

        private TemperatureInterface TemperatureInterface;

        private ISettingService SettingService;

        private LicenseService LicenseService;
        private FridgeLockInterface FridgeLockInterface;

        private ISendMessageToCloudService SendMessageToCloudService;
        private ITemperatureService TemperatureService;
        private IFridgePayment FridgePayment;
        private CustomerUINotificationService CustomerUINotificationService;
        private MachineStatusService MachineStatusService;
        private CameraInterface CameraInterface;
        private UnstableTagService UnstableTagService;
        private IBlacklistCardsService BlacklistCardsService;
        private QrPaymentService QrPaymentService;
        private CmdExecuteService CmdExecuteService;
        private PayterInterface PayterInterface;
        private DeviceCheckingService DeviceCheckingService;
        private QrReaderTtlInterface QrReaderTtlInterface;

        private string MachineName { get; set; }
        private bool UseCloud { get; set; }

        public TestApplication(
              LogService logService,
              FridgeInterface fridgeInterface,
              RabbitMqService rabbitMqService,
              StompService stompService,
              WebApiService webApiService,
              ITransactionService transactionService,
              SlackService slackService,
              TemperatureInterface temperatureInterface,
              ISettingService settingService,
              LicenseService licenseService,
              ISendMessageToCloudService sendMessageToCloudService,
              ITemperatureService temperatureService,
              NayaxInterface nayaxInterface,
              IFridgePayment fridgePayment,
              CustomerUINotificationService customerUINotificationService,
               FridgeLockInterface fridgeLockInterface,
               MachineStatusService machineStatusService,
               CameraInterface cameraInterface,
               UnstableTagService unstableTagService,
               IBlacklistCardsService blacklistCardsService,
               QrPaymentService qrPaymentService,
               CmdExecuteService cmdExecuteService,
               PayterInterface payterInterface,
               DeviceCheckingService deviceCheckingService,
               QrReaderTtlInterface qrReaderTtlInterface

            )
        {
            LogService = logService;
            FridgeInterface = fridgeInterface;
            RabbitMqService = rabbitMqService;
            WebApiService = webApiService;
            TransactionService = transactionService;
            SlackService = slackService;
            TemperatureInterface = temperatureInterface;
            StompService = stompService;
            SettingService = settingService;
            LicenseService = licenseService;
            SendMessageToCloudService = sendMessageToCloudService;
            TemperatureService = temperatureService;
            NayaxInterface = nayaxInterface;
            FridgePayment = fridgePayment;
            CustomerUINotificationService = customerUINotificationService;
            FridgeLockInterface = fridgeLockInterface;
            MachineStatusService = machineStatusService;
            CameraInterface = cameraInterface;
            UnstableTagService = unstableTagService;
            BlacklistCardsService = blacklistCardsService;
            QrPaymentService = qrPaymentService;
            CmdExecuteService = cmdExecuteService;
            PayterInterface = payterInterface;
            DeviceCheckingService = deviceCheckingService;
            QrReaderTtlInterface = qrReaderTtlInterface;
        }


        public void Run()
        {
            LogService.Init();

            // PayterInterface.Connect("COM7");

            // PayterInterface.InitTerminal(20);

           // QrReaderTtlInterface.Connect("COM28");
           // QrReaderTtlInterface.CheckStatus();


            Thread.Sleep(500);

            // PayterInterface.VendRequest(1);
            // Thread.Sleep(500);
            //PayterInterface.Sync();
            Console.ReadLine();
        }
        public void StartWebApi()
        {
            var portName = 9000;
            var url = $"http://*:{portName}/";
            try
            {
                WebApp.Start<Startup>(url);
                Console.WriteLine("API started at:" + url);
            }
            catch (TaskCanceledException)
            {
                this.LogService.LogInfo("Machine start timeout, please restart application.");
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex);
            }
        }
        public void RunAsWebService()
        {
            var portName = 9000;
            var url = $"http://*:{portName}/";
            try
            {
                using (WebApp.Start<Startup>(url))
                {
                    Console.WriteLine("Server started at:" + url);
                    Console.WriteLine("Machine is starting");
                    var client = new HttpClient();
                    var response = client.GetAsync($"http://localhost:{portName}/api/machine/run").Result;
                    var result = response.Content.ReadAsAsync<string>().Result;
                    Console.WriteLine($"Start Result: {result}");
                    Console.ReadLine();
                }
            }
            catch (TaskCanceledException)
            {
                this.LogService.LogInfo("Machine start timeout, please restart application.");
                Console.Read();
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex);
                Console.Read();
            }

            //while (true)
            //{
            //    var text = Console.ReadLine();
            //    if (text == "X")
            //    {
            //        Environment.Exit(0);
            //    }
            //}
        }
    }
}