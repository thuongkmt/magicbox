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

    public class Application
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

        public string AppVersion => $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}({AppBuildDate})";
        public string AppBuildDate => $"{System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location).ToString("dd/MM")}";


        public Application(
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
           // var sss = new GrabpPayInterface(null, new GrabPayService(), null, null);
           // sss.Inquiry("partner-9grl0chhb9rahce22x08evvl").Wait();

            var grab = new GrabPayService();
            string timestamp = DateTime.UtcNow.ToString("ddd, dd MMM yyy HH:mm:ss 'GMT'");
            string timestamp1 = DateTime.MinValue.ToString("ddd, dd MMM yyy HH:mm:ss 'GMT'");


            var TERMINAL_ID = "2af6400b6485cc619cc11cadc";
            var GRAB_ID = "6cead152-561f-4373-b3cb-5dc23538215f";
            var CURRENCY_CODE = "SGD";

            var partnerTxID = "partner-vxowlf325vu23ueisey1sno2";
            var msgID = "6l94g39z57xjuapmujyp28x0jyenvbfm";

            var amount1 = "1";
            var code1 = "6569423850974433291";
            var time1 = "Fri, 06 Aug 2021 09:24:48 GMT";

            string requestBody = "{\n\t\"amount\":" + amount1 +
           ",\n\t\"msgID\":\"" + msgID +
           "\",\n\t\"grabID\":\"" + GRAB_ID +
           "\",\n\t\"terminalID\":\"" + TERMINAL_ID +
           "\",\n\t\"currency\":\"" + CURRENCY_CODE +
           "\",\n\t\"partnerTxID\":\"" + partnerTxID +
           "\",\n\t\"code\":\"" + code1 +
           "\"\n}";

            var hmac = grab.GenerateHMACSignature("c589284f-39be-4351-b29f-d44b02ddb56d", "U0OaTpmGJtV0EYkI", "POST", "https://partner-gateway.grab.com/grabpay/partner/v1/terminal/transaction/perform",
                "application/json", requestBody, time1);
            grab.ParseURL("https://partner-api.stg-myteksi.com/grabpay/partner/v1/terminal/qrcode/create", "");

            var IsDebug = System.Diagnostics.Debugger.IsAttached;

            #region INIT

            LogService.Init();
            var path = AppDomain.CurrentDomain.BaseDirectory;

        //Validate licsence first
        CHECK_LIC:
            if (!LicenseService.IsValid())
            {
                Console.WriteLine("Please input product key");
                var productKey = Console.ReadLine();
                this.LicenseService.Registration(productKey);
                goto CHECK_LIC;
            }

            LogService.LogInfo("RFID Fridge start on path: " + path);
            LogService.LogInfo("Version: " + AppVersion);


            // Web Api Data
            var webApiUrl = System.Configuration.ConfigurationManager.AppSettings.Get("WebApi_Url");
            var webApiUserName = System.Configuration.ConfigurationManager.AppSettings.Get("WebApi_UserName");
            var webApiPassword = System.Configuration.ConfigurationManager.AppSettings.Get("WebApi_Password");
            WebApiService.Init(webApiUrl, webApiUserName, webApiPassword);

            StartWebApi();

        RESTART:

            CustomerUINotificationService.StartUi();


        WAIT_FOR_API_SERVER:
            // Get all settings
            // Use RfidFridgeSetting class as global setting
            bool r = SettingService.GetAll().Result;
            if (!r)
            {
                LogService.LogInfo("Try to connect to API Server in 10s");

                Thread.Sleep(10000);
                LogService.LogInfo("Retrying to reconnect to API Server");
                goto WAIT_FOR_API_SERVER;
            }

            // Set machine name
            MachineName = RfidFridgeSetting.Machine.Name ?? "MAGIC_BOX";

            bool.TryParse(RfidFridgeSetting.System.Cloud.UseCloud, out bool useCloud);
            UseCloud = useCloud;
            var machineNameNoSpace = RfidFridgeSetting.Machine.Name.Replace(" ", string.Empty);
            if (UseCloud)
            {
                SendMessageToCloudService.InitConfigAndConnect(
                    RfidFridgeSetting.System.Cloud.RabbitMqServer,
                    RfidFridgeSetting.System.Cloud.RabbitMqUser,
                    RfidFridgeSetting.System.Cloud.RabbitMqPassword,
                    $"magicbox-{machineNameNoSpace}-{RfidFridgeSetting.Machine.Id}");
            }



            // Slack
            this.SlackService.Init(
                RfidFridgeSetting.System.Slack.InfoChannel,
                RfidFridgeSetting.System.Slack.AlertChannel,
                RfidFridgeSetting.System.Slack.UnstableTagChannel,

                RfidFridgeSetting.System.Slack.Username,
                RfidFridgeSetting.System.Slack.URL);

            var rabbitMqTryTime = 0;

        WAIT_FOR_RABBIT_MQ:

            LogService.LogInfo("Wait for RabbitMQ start.");
            var result = RabbitMqService.Init();
            if (!result)
            {

                if (++rabbitMqTryTime >= 30)
                {
                    SlackService.SendAlert(RfidFridgeSetting.Machine.Name, "RabbitMQ is not started!!!");
                    return;
                }
                else
                {
                    //LogService.LogInfo("RabbitMQ is not started, trying to reconnect!!");
                    Thread.Sleep(5000);
                    goto WAIT_FOR_RABBIT_MQ;
                }
            }
            LogService.LogInfo("RabbitMQ started.");


            CustomerUINotificationService.WaitForUiStart(MachineStatus.UI_STARTED);
            CustomerUINotificationService.PublishScreenBaseOnStatus(MachineStatus.DEVICE_CHECKING);
            CustomerUINotificationService.WaitForUiStart(MachineStatus.DEVICE_CHECKING);
            // Stomp command
            Task.Run(() =>
            {
                StompService.Connect();
                //Thread.Sleep(1000);
                this.StompService.OnCommandRev = this.OnStpCmdRev;
                StompService.SubCommand();
            });

            // Init Fridge
            int.TryParse(RfidFridgeSetting.System.Inventory.Delay, out int inventoryAdditionalDelay);

            // Antenna
            if (string.IsNullOrEmpty(RfidFridgeSetting.System.Inventory.Antenna))
            {
                FridgeInterface.AntennaList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            }
            else
            {
                var antenna = RfidFridgeSetting.System.Inventory.Antenna.ToString().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.Parse(x))
                    .OrderBy(x => x)
                    .ToList();
                FridgeInterface.AntennaList = antenna;
            }
            int.TryParse(RfidFridgeSetting.System.Inventory.ReaderVersion, out int readerVersion);
            FridgeInterface.InventoryComport = RfidFridgeSetting.System.Comport.Inventory;
            FridgeInterface.LockComport = RfidFridgeSetting.System.Comport.Lock;
            FridgeInterface.InventoryAdditionalDelay = (inventoryAdditionalDelay == 0) ? 2000 : inventoryAdditionalDelay;
            FridgeInterface.InventoryReaderVersion = (FridgeReaderVersion)readerVersion;
            FridgeInterface.Init(path);


            CmdExecuteService.Init();
            #endregion INIT

            #region Payment type
            // Get payment type
            Enum.TryParse(RfidFridgeSetting.System.Payment.Type?.ToUpper(), out Domain.Enums.PaymentType supportPaymentType);
            Enum.TryParse(RfidFridgeSetting.System.Payment.Qr.Type?.ToUpper(), out Domain.Enums.QrPaymentType qrPaymentType);

            var paymentTypes = supportPaymentType.ToString().Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            LogService.LogInfo("PAYMENT TYPE: " + string.Join(",", paymentTypes));



            var iucErrorCodes = new List<string>();

            //QrPaymentService.Init(qrPaymentType);


            foreach (var p in paymentTypes)
            {
                Enum.TryParse(p.ToUpper(), out Domain.Enums.PaymentType paymentType);
                switch (paymentType)
                {
                    case Domain.Enums.PaymentType.NAYAX:
                        // Init terminal
                        LogService.LogInfo("Starting Terminal: " + RfidFridgeSetting.System.Comport.CashlessTerminal);
                        NayaxInterface.Connect(RfidFridgeSetting.System.Comport.CashlessTerminal);
                        NayaxInterface.OnValidateCardSuccess = FridgeInterface.StartTransaction;
                        break;
                    case Domain.Enums.PaymentType.MAGIC:
                        FridgePayment.Connect(RfidFridgeSetting.System.Comport.CashlessTerminal);
                        FridgePayment.OnValidateCardSuccess = FridgeInterface.StartTransaction;
                        FridgePayment.Start();
                        //iucErrorCodes = RfidFridgeSetting.System.Payment.Magic.RetryToChargeIfGotErrorCodes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        //LogService.LogInfo("Retry if got those error codes: " + string.Join(",", iucErrorCodes));
                        break;
                    case Domain.Enums.PaymentType.QR:
                        QrReaderTtlInterface.Connect(RfidFridgeSetting.System.Comport.QrCodeReader);
                        LogService.LogInfo("QR PAYMENT TYPE: " + qrPaymentType);

                        QrPaymentService.Init(qrPaymentType);
                        QrPaymentService.OnValidateQrSuccess = FridgeInterface.StartTransaction;
                        QrPaymentService.Start();
                        break;
                    case Domain.Enums.PaymentType.PAYTER:
                        LogService.LogInfo("Starting Payter Terminal: " + RfidFridgeSetting.System.Comport.CashlessTerminal);
                        PayterInterface.Connect(RfidFridgeSetting.System.Comport.CashlessTerminal);
                        int.TryParse(RfidFridgeSetting.System.Payment.Payter.PreAuthAmount, out int payterreserveAmount);
                        PayterInterface.InitTerminal(payterreserveAmount);
                        PayterInterface.OnValidateCardSuccess = FridgeInterface.StartTransaction;
                        break;
                }
            }


            FridgePayment.OnValidateCardFailed = () =>
            {
                LogService.LogInfo("Change to validate card failed screen!");
                //StompService.PublishMachineStatus(MachineStatus.VALIDATE_CARD_FAILED);
                FridgeInterface.CurrentMachineStatus = MachineStatus.VALIDATE_CARD_FAILED;
                Thread.Sleep(10000);
                //StompService.PublishMachineStatus(MachineStatus.IDLE);
                FridgeInterface.CurrentMachineStatus = MachineStatus.IDLE;
            };

            #endregion

            #region Fridge Events



            // Event/Workflow
            FridgeInterface.OnMachineStatusChange = (status) =>
            {
                if (_machineStatus != status)
                {
                    this.LogService.LogInfo($"MACHINE STATUS: {status}");
                    try
                    {
                        if (status == MachineStatus.STOPSALE || status == MachineStatus.MANUAL_STOPSALE)

                        {
                            foreach (var p in paymentTypes)
                            {
                                Enum.TryParse(p.ToUpper(), out Domain.Enums.PaymentType paymentType);

                                switch (paymentType)
                                {
                                    case Domain.Enums.PaymentType.NAYAX:
                                        NayaxInterface.DisableReader();
                                        break;
                                    case Domain.Enums.PaymentType.PAYTER:
                                        break;
                                    case Domain.Enums.PaymentType.MAGIC:
                                        FridgePayment.End();
                                        break;
                                    case Domain.Enums.PaymentType.QR:
                                        QrPaymentService.End();
                                        break;
                                }
                            }
                        }

                        //this.LogService.LogInfo("Machine status: " + _machineStatus);
                        if (status == MachineStatus.IDLE)
                        {
                            CustomerUINotificationService.TapCard();

                            foreach (var p in paymentTypes)
                            {
                                Enum.TryParse(p.ToUpper(), out Domain.Enums.PaymentType paymentType);

                                this.LogService.LogInfo("Payment type: " + paymentType);
                                switch (paymentType)
                                {
                                    case Domain.Enums.PaymentType.NAYAX:
                                        this.LogService.LogInfo("Enable Nayax");
                                        NayaxInterface.EnableReader();
                                        break;
                                    case Domain.Enums.PaymentType.MAGIC:
                                        this.LogService.LogInfo("Magic Payment Start");
                                        FridgePayment.Start();
                                        break;
                                    case Domain.Enums.PaymentType.QR:
                                        this.LogService.LogInfo("QR Payment Start");
                                        QrPaymentService.Start();
                                        break;
                                    case Domain.Enums.PaymentType.PAYTER:
                                        this.LogService.LogInfo("Enable Payter");
                                        //NayaxInterface.EnableReader();
                                        PayterInterface.EnableReader();
                                        int.TryParse(RfidFridgeSetting.System.Payment.Payter.PreAuthAmount, out int payterPreauthAmount);
                                        PayterInterface.VendRequestV3(payterPreauthAmount);

                                        break;
                                }
                            }
                        }

                        if (status == MachineStatus.FAIL_TO_OPEN_THE_DOOR)
                        {
                            CustomerUINotificationService.FailToOpenTheDoor();

                            foreach (var p in paymentTypes)
                            {
                                Enum.TryParse(p.ToUpper(), out Domain.Enums.PaymentType paymentType);

                                switch (paymentType)
                                {
                                    case Domain.Enums.PaymentType.MAGIC:
                                        LogService.LogInfo("End payment due to failed to open the door");
                                        FridgePayment.End();
                                        Thread.Sleep(10000);
                                        FridgeInterface.CurrentMachineStatus = MachineStatus.IDLE;
                                        FridgePayment.Start();
                                        break;
                                }
                            }

                        }

                        _machineStatus = status;

                    }
                    catch (Exception ex)
                    {
                        LogService.LogError(ex);
                    }

                    CustomerUINotificationService.PublishScreenBaseOnStatus(status);

                    Task.Run(() =>
                    {
                        MachineStatusService.NotifyChangeToMachineStatus(FridgeInterface.CurrentMachineStatus, nameof(FridgeInterface.CurrentMachineStatus));
                    });

                }
            };

            FridgeInterface.OnPreauthTxnFinish = (inventories, amount, images) =>
            {

                if (amount > 0)
                {
                    LogService.LogInfo($"Charing...");
                    // Change to screen make payment...
                    FridgeInterface.CurrentMachineStatus = MachineStatus.PREAUTH_MAKE_PAYMENT;
                    FridgePayment.Charge(amount, (status, saleResponse, cardType, cardNumber) =>
                    {
                        LogService.LogInfo($"Charing finished | Amount: {amount} | Status: {status} | Card Type: {cardType}");
                        if (status == TransactionStatus.Error)
                        {

                            OnTransactionFinished(inventories, amount);
                            AddTransaction(inventories, status, saleResponse, PaymentType.MAGIC, cardType, images, amount);
                        }
                        else
                        {
                            LogService.LogInfo($"Refunding...");
                            FridgeInterface.CurrentMachineStatus = MachineStatus.PREAUTH_REFUND;
                            FridgePayment.Refund((isSuccess) =>
                            {
                                LogService.LogInfo($"Refund success = " + isSuccess);
                                OnTransactionFinished(inventories, amount);
                                AddTransaction(inventories, status, saleResponse, PaymentType.MAGIC, cardType, images, amount);
                            });
                        }
                    });
                }
                else if (amount == 0)
                {
                    LogService.LogInfo($"Refunding...");
                    FridgeInterface.CurrentMachineStatus = MachineStatus.PREAUTH_REFUND;
                    FridgePayment.Refund((isSuccess) =>
                    {
                        LogService.LogInfo($"Refund success = " + isSuccess);
                        OnTransactionFinished(inventories, amount);
                        AddTransaction(inventories, TransactionStatus.Cancelled, null, PaymentType.MAGIC, CardPaymentType.CREDITCARD, images, amount);
                    });
                }
                else
                {
                    OnTransactionFinished(inventories, amount);
                }
            };

            var isTransactionEnding = false;
            FridgeInterface.OnTransactionReadyForPayment = (inventories, amount, images) =>
            {
                if (isTransactionEnding)
                {
                    LogService.LogInfo("Transaction is ending, skip");
                    return;
                }
                isTransactionEnding = true;
                try
                {
                    CustomerUINotificationService.PleaseWait();
                    if (!FridgeInterface.IsTestTransaction)
                    {
                        var paymentType = FridgeInterface.SelectedPaymentMode;
                        LogService.LogInfo($"Transaction Ready For Payment: {paymentType}");
                        LogService.LogInfo($"Transaction Amount: {amount}");

                        switch (paymentType)
                        {
                            case Domain.Enums.PaymentType.NAYAX:
                                int.TryParse(RfidFridgeSetting.System.Payment.Nayax.PreAuthAmount, out int nayaxPreauthAmount);
                                var tnxAmount = 0;
                                TransactionStatus txStatus = TransactionStatus.Success;
                                if (amount > nayaxPreauthAmount)
                                {
                                    tnxAmount = NayaxInterface.Amount = nayaxPreauthAmount;
                                    txStatus = TransactionStatus.Error;
                                    Task.Run(() =>
                                    {
                                        SlackService.SendAlert(MachineName, "[NAYAX] transaction amount is great than Pre Auth amount!!");
                                    });
                                }
                                else
                                {
                                    tnxAmount = NayaxInterface.Amount = amount;
                                }

                                if (this.NayaxInterface.Amount > 0)
                                {
                                    this.NayaxInterface.VendRequest();
                                }
                                else
                                {
                                    this.NayaxInterface.VendCancel();
                                }
                                OnTransactionFinished(inventories, tnxAmount);
                                AddTransaction(inventories, txStatus, null, PaymentType.NAYAX, CardPaymentType.CREDITCARD, images, amount);
                                break;
                            case Domain.Enums.PaymentType.MAGIC:
                                if (amount > 0)
                                {

                                    FridgePayment.Charge(amount, (status, saleResponse, cardType, cardNumber) =>
                                    {
                                        LogService.LogInfo($"Charing finished | Amount: {amount} | Status: {status} | Card Type: {cardType}");
                                        if (status == TransactionStatus.Error)
                                        {
                                            // Disable sale?
                                            bool.TryParse(RfidFridgeSetting.System.Payment.Magic.StopSaleDueToPayment, out bool disableSaleIfError);
                                            var response = (SaleResponse)saleResponse;
                                            LogService.LogInfo($"Charing Error Code: {response.ResponseCode}");


                                            if (response.ResponseCode == "BL")
                                            {
                                                Task.Run(() =>
                                                {
                                                    BlacklistCardsService.Add(new BlackListCardsDto(cardNumber, cardType.ToString(), amount / 100m));
                                                    SlackService.SendAlert(RfidFridgeSetting.Machine.Name, $"Found CPAS Black List card, adding to BlackList | Card Number: {cardNumber}");
                                                });
                                            }

                                            if (disableSaleIfError)
                                            {


                                                var err = $"Disable sale due to payment error | Amount: {amount} | Status: {status} | Card Type: {cardType} | Error: [{response.ResponseCode}] {response.Message}";
                                                LogService.LogInfo(err);
                                                SlackService.SendAlert(MachineName, err);
                                                FridgeInterface.CurrentMachineStatus = MachineStatus.STOPSALE_DUE_TO_PAYMENT;
                                                SlackService.SendAlert(MachineName, "CARD IS LOCKED");
                                                SlackService.SendInfo(MachineName, "CARD IS LOCKED");
                                            }
                                            else
                                            {
                                                OnTransactionFinished(inventories, amount);
                                            }
                                        }
                                        else
                                        {
                                            OnTransactionFinished(inventories, amount);
                                        }

                                        AddTransaction(inventories, status, saleResponse, PaymentType.MAGIC, cardType, images, amount);
                                    });
                                }
                                else
                                {
                                    FridgePayment.End();
                                    OnTransactionFinished(inventories, amount);
                                    AddTransaction(inventories, TransactionStatus.Success, null, PaymentType.MAGIC, CardPaymentType.NONE, images, amount);
                                }
                                break;

                            case Domain.Enums.PaymentType.QR:
                                {
                                    QrPaymentService.Charge(amount, (status, saleResponse) =>
                                    {
                                        LogService.LogInfo($"Charing finished | Amount: {amount} | Status: {status}");
                                        OnTransactionFinished(inventories, amount);

                                        CardPaymentType paymentType2 = CardPaymentType.NONE;
                                        if (qrPaymentType == QrPaymentType.GRABPAY)
                                        {
                                            paymentType2 = CardPaymentType.GRABPAY;
                                        }
                                        if (qrPaymentType == QrPaymentType.WALLET)
                                        {
                                            paymentType2 = CardPaymentType.WALLET;
                                        }
                                        if (qrPaymentType == QrPaymentType.CREDITCARD_WALLET)
                                        {
                                            paymentType2 = CardPaymentType.CREDITCARD_WALLET;
                                        }
                                        AddTransaction(inventories, status, saleResponse, PaymentType.QR, paymentType2, images, amount);
                                        QrPaymentService.End();
                                    }, inventories);
                                }



                                break;

                            case Domain.Enums.PaymentType.PAYTER:
                                int.TryParse(RfidFridgeSetting.System.Payment.Payter.PreAuthAmount, out int payterPreauthAmount);
                                var ptnxAmount = 0;
                                TransactionStatus ptxStatus = TransactionStatus.Success;
                                if (amount >= payterPreauthAmount)
                                {
                                    ptnxAmount = payterPreauthAmount;
                                    ptxStatus = TransactionStatus.Error;
                                    Task.Run(() =>
                                    {
                                        SlackService.SendAlert(MachineName, "[PAYTER] transaction amount is great than Pre Auth amount!!");
                                    });
                                }
                                else
                                {
                                    ptnxAmount = amount;
                                }


                                LogService.LogInfo($"Payter Amount: {ptnxAmount}");

                                if (ptnxAmount > 0)
                                {
                                    this.PayterInterface.VendSuccess(ptnxAmount);
                                    this.PayterInterface.SessionComplete();
                                }
                                else
                                {
                                    this.PayterInterface.VendCancel();
                                }
                                OnTransactionFinished(inventories, ptnxAmount);
                                AddTransaction(inventories, ptxStatus, null, PaymentType.PAYTER, CardPaymentType.CREDITCARD, images, amount);
                                break;
                        }
                    }
                    else
                    {
                        FridgeInterface.IsTestTransaction = false;
                        OnTransactionFinished(inventories, amount);
                        AddTransaction(inventories, TransactionStatus.Test, null, PaymentType.NONE, CardPaymentType.NONE, images, amount);
                    }
                }
                catch (Exception ex)
                {
                    isTransactionEnding = false;
                    LogService.LogError(ex);
                }

                // Add transaction to local admin

            };

            void AddTransaction(List<InventoryDto> items, TransactionStatus status, object saleResponse, PaymentType payment, CardPaymentType card, List<string> transactionImages, int amount)
            {
                Task.Run(() =>
                {
                    try
                    {
                        var productName = items.Select(x => x.Product.ProductName);
                        var transactionInfo = string.Join(", ", productName);
                        //var amount = inventories.Sum(x => x.Price);

                        var slackMess = string.Empty;
                        if (status == TransactionStatus.Success)
                        {
                            slackMess = (items.Count > 0)
                                            ? RfidFridgeSetting.Alert.Messages.TransactionCompleted
                                                .Replace("{status}".ToString(), status.ToString())
                                                .Replace("{amount}".ToString(), amount.ToString())
                                                .Replace("{products}".ToString(), transactionInfo.ToString())
                                            : RfidFridgeSetting.Alert.Messages.TransactionCancelled;
                            SlackService.SendInfo(MachineName, $"{slackMess}");
                        }
                        else
                        {
                            slackMess = RfidFridgeSetting.Alert.Messages.TransactionError
                                                .Replace("{status}".ToString(), status.ToString())
                                                .Replace("{amount}".ToString(), amount.ToString())
                                                .Replace("{products}".ToString(), transactionInfo.ToString());
                            SlackService.SendAlert(MachineName, $"{slackMess}");
                        }

                        TransactionService.Add(items, status, saleResponse, payment, card, transactionImages);
                        FridgeInterface.InitInventoryData();

                        // CALLBACK
                        StompService.PublishTransactionCompleted("TRANSACTION_COMPLETED");

                        isTransactionEnding = false;
                    }
                    catch (Exception ex)
                    {
                        isTransactionEnding = false;
                        LogService.LogError(ex);
                    }
                });
            }

            void OnTransactionFinished(List<InventoryDto> inventories1, int amount)
            {
                try
                {
                    FridgeInterface.CurrentMachineStatus = MachineStatus.TRANSACTION_DONE;
                    var paymentType = FridgeInterface.SelectedPaymentMode;
                    LogService.LogInfo("Transaction finished, payment mode: " + paymentType);
                    SlackService.SendInfo(MachineName, "Transaction Finished.");
                    switch (paymentType)
                    {
                        case Domain.Enums.PaymentType.PAYTER:
                            Thread.Sleep(5000);
                            LogService.LogInfo("EndTransaction IDLE");
                            FridgeInterface.CurrentMachineStatus = MachineStatus.IDLE;
                            break;
                        case Domain.Enums.PaymentType.NAYAX:
                            Thread.Sleep(5000);
                            LogService.LogInfo("EndTransaction IDLE");
                            FridgeInterface.CurrentMachineStatus = MachineStatus.IDLE;
                            break;
                        case Domain.Enums.PaymentType.MAGIC:
                            Thread.Sleep(5000);

                            //if (amount > 0)
                            //{
                            //    var tryTime = 1;
                            //    while (true)
                            //    {
                            //        LogService.LogInfo($"FridgePayment.IsChargeFinished: {FridgePayment.IsChargeFinished}");
                            //        if (FridgePayment.IsChargeFinished)
                            //        {
                            //            LogService.LogInfo("Charging has been finished");
                            //            break;
                            //        }

                            //        LogService.LogInfo("Wait for transaction finish: " + tryTime);
                            //        if (++tryTime > 90)
                            //        {
                            //            LogService.LogInfo("Wait for transaction finish TIMEOUT!!!!");
                            //            break;
                            //        }

                            //        Thread.Sleep(1000);
                            //    }
                            //}

                            if (FridgeInterface.CurrentMachineStatus == MachineStatus.TRANSACTION_DONE)
                            {
                                LogService.LogInfo("End Transaction IDLE");
                                FridgeInterface.CurrentMachineStatus = MachineStatus.IDLE;
                            }
                            else
                            {
                                LogService.LogInfo("Can not return to IDLE: " + FridgeInterface.CurrentMachineStatus);
                            }
                            break;


                        case Domain.Enums.PaymentType.QR:
                            Thread.Sleep(5000);
                            LogService.LogInfo("End QR Transaction");
                            FridgeInterface.CurrentMachineStatus = MachineStatus.IDLE;
                            break;
                    }

                    //  Unmap tags
                    //bool.TryParse(RfidFridgeSetting.System.Inventory.RemoveTagAfterSold, out bool removeAfterSold);
                    //if (removeAfterSold)
                    //{
                    //    var ids = inventories1.Select(x => x.Id).ToList();
                    //    InventoryService.RemoveByIds(ids);
                    //}

                    //FridgeInterface.InitInventoryData();

                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            }

            FridgeInterface.ReloadInventoryCallback = () =>
            {
                if (UseCloud)
                {
                    var inventories = FridgeInterface.GetCurrentInventory();
                    LogService.LogInfo("Report inventories on callback: " + inventories.Count());
                    var d = inventories.Select(x => new KeyValuePair<string, string>(x.TagId, x.Product.ProductName));
                    var kv = new KeyValueMessage()
                    {
                        Key = MessageKeys.ProductRfidTagsRealtime,
                        MachineId = Guid.Parse(RfidFridgeSetting.Machine.Id),
                        Value = d
                    };

                    SendMessageToCloudService.SendMsgToCloud(kv);
                }
            };
            string OldOrderHashTagId = string.Empty;

            FridgeInterface.OnTxnOrderChanged = (order) =>
            {
                var paymentType = FridgeInterface.SelectedPaymentMode;
                var reserveAmount = 2000;
                if (paymentType == PaymentType.MAGIC)
                {
                    int.TryParse(RfidFridgeSetting.System.Payment.Magic.MinBalanceRequire, out reserveAmount);
                }
                else if (paymentType == PaymentType.NAYAX)
                {
                    int.TryParse(RfidFridgeSetting.System.Payment.Nayax.PreAuthAmount, out reserveAmount);
                }
                else if (paymentType == PaymentType.QR)
                {
                    int.TryParse(RfidFridgeSetting.System.Payment.GrabPay.ReserveAmount, out reserveAmount);
                }
                var orderTagId = order.Inventories.Select(x => x.TagId).OrderBy(x => x);
                var orderHashTagId = string.Join("|", orderTagId);
                if (orderHashTagId != OldOrderHashTagId)
                {
                    var orderMessage = string.Join(", ", order.Inventories.Select(x => $"[{x.Product.ProductName}|{x.TagId}]"));
                    SlackService.SendInfo(RfidFridgeSetting.Machine.Name, "Items removed: " + orderMessage);
                    OldOrderHashTagId = orderHashTagId;
                }
                CustomerUINotificationService.DoNotTakeMoreItem(order, reserveAmount);
            };
            #endregion Fridge Events

            #region Tags
            FridgeInterface.OnTagChange = (tags, e) =>
            {
                if (tags.Count == FridgeInterface.GetCurrentInventory().Count)
                {
                    return;
                }
                var inventoryMess = "";
                var listMessage = new List<string>();
                var stag = string.Join("|", tags);
                LogService.LogInfo($"TAGS {e}: {stag} ");
                foreach (var tag in tags)
                {
                    var inventory = FridgeInterface.Inventories.FirstOrDefault(x => x.TagId.Trim() == tag.Trim());

                    var msg = string.Empty;
                    if (inventory != null)
                    {
                        msg = $"[{inventory.Product.ProductName} | {tag}]";
                    }
                    else
                    {
                        msg = $"[TAG | {tag}]";
                    }
                    listMessage.Add(msg);
                    inventoryMess = string.Join(",", listMessage);
                }
                LogService.LogInfo($"Inventory {e}: {inventoryMess} ");

                if (FridgeInterface.CurrentDoorState == DoorState.CLOSE || IsDebug)
                {
                    if (!string.IsNullOrEmpty(RfidFridgeSetting.Alert.Messages.UnstableTag))
                    {
                        if (e == TagChangeEvent.ADDED)
                        {
                            var message = RfidFridgeSetting.Alert.Messages.UnstableTag.Replace("{tags}".ToString(), inventoryMess);
                            LogService.LogInfo(message);
                            UnstableTagService.Record(tags);
                            SlackService.SendUnstableTagAlert(MachineName, message);
                        }
                    }
                }
            };

            FridgeInterface.OnReportInventory = (inventories) =>
            {
                Task.Run(() =>
                {
                    try
                    {
                        if (UseCloud)
                        {
                            if (FridgeInterface.CurrentDoorState == DoorState.OPEN)
                            {
                                LogService.LogInfo("Report inventories: " + inventories.Count);
                                //var s = inventories.Select(x => (x.Product + "," + x.TagId));
                                //LogService.LogInfo(String.Join("|", s));
                                var d = inventories.Select(x => new KeyValuePair<string, string>(x.TagId, x.Product.ProductName));
                                var kv = new KeyValueMessage()
                                {
                                    Key = MessageKeys.ProductRfidTagsRealtime,
                                    MachineId = Guid.Parse(RfidFridgeSetting.Machine.Id),
                                    Value = d
                                };

                                SendMessageToCloudService.SendMsgToCloud(kv);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogService.LogError(ex);
                    }
                });
            };



            #endregion Tags

            #region Door

            var timerAlarmDoorOpen = new System.Timers.Timer();
            timerAlarmDoorOpen.Elapsed += TimerAlarmDoorOpen_Elapsed;

            bool isDoorOpenTooLongAlarm = false;
            void TimerAlarmDoorOpen_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                isDoorOpenTooLongAlarm = true;
                int.TryParse(RfidFridgeSetting.Alert.DoorOpenTooLongAlertRepeatTime, out int interval);

                timerAlarmDoorOpen.Interval = interval == 0 ? 10000 : interval;
                LogService.LogInfo("Sending door open too long alarm!!");
                SlackService.SendAlert(MachineName, RfidFridgeSetting.Alert.Messages.DoorOpenTooLong);
                CustomerUINotificationService.DoorOpenTooLong();

                if (FridgeInterface.CurrentDoorState == DoorState.CLOSE)
                {
                    CustomerUINotificationService.DismissDialog();
                    CustomerUINotificationService.StopSound();
                    LogService.LogInfo("Door closed / Stop sending door open too long alarm.");
                    timerAlarmDoorOpen.Enabled = false;
                    timerAlarmDoorOpen.Stop();
                }
            }

            // TODO: Reflector to FridgeLockInterface
            FridgeInterface.OnDoorStateChange = (s) =>
            {
                var status = s == DoorState.OPEN
                                             ? RfidFridgeSetting.Alert.Messages.DoorOpen
                                             : RfidFridgeSetting.Alert.Messages.DoorClose;
                LogService.LogInfo(status);
                if (s == DoorState.OPEN)
                {
                    timerAlarmDoorOpen.Enabled = false;
                    timerAlarmDoorOpen.Stop();

                    int.TryParse(RfidFridgeSetting.Alert.DoorOpenTimeOut, out int interval);
                    timerAlarmDoorOpen.Interval = interval == 0 ? 60000 : interval;
                    timerAlarmDoorOpen.Enabled = true;
                    timerAlarmDoorOpen.Start();
                }
                else if (s == DoorState.CLOSE)
                {
                    timerAlarmDoorOpen.Enabled = false;
                    timerAlarmDoorOpen.Stop();

                    if (isDoorOpenTooLongAlarm)
                    {
                        CustomerUINotificationService.DismissDialog();
                        LogService.LogInfo("Door closed / Stop sending door open too long alarm.");
                    }
                }

                MachineStatusService.NotifyChangeToMachineStatus(FridgeInterface.CurrentDoorState, nameof(FridgeInterface.CurrentDoorState));

                Task.Run(() => { SlackService.SendInfo(MachineName, status); });
            };

            this.FridgeLockInterface.OnDoorAlarm = (s) =>
            {
                SlackService.SendAlert(MachineName, s);
            };

            #endregion Door

            #region Temperature

            Boolean.TryParse(RfidFridgeSetting.System.Temperature.Enable, out bool enableTemp);
            var tmpEnable = (enableTemp == true) ? "ENABLE" : "DISABLE";
            LogService.LogInfo($"Temperature is {tmpEnable}");
            LogService.LogInfo($"Temperature interface: {RfidFridgeSetting.System.Temperature.Comport}");

            if (enableTemp)
            {
                int.TryParse(RfidFridgeSetting.System.Temperature.ReportInterval, out int tmpInterval);
                int.TryParse(RfidFridgeSetting.System.Temperature.ReportToCloudInterval, out int tmpCloudInterval);
                int.TryParse(RfidFridgeSetting.System.Temperature.NormalTemperature, out int temperature);
                int.TryParse(RfidFridgeSetting.System.Temperature.SourceIndex, out int temperatureIndex);
                int.TryParse(RfidFridgeSetting.System.Temperature.Offset, out int temperatureOffset);

                var tempInterface = RfidFridgeSetting.System.Temperature.Comport;
                //tempInterface = "COM8";
                if (tempInterface.Equals("HID", StringComparison.OrdinalIgnoreCase))
                {
                    TemperatureInterface.ThermalProbeType = TemperatureInterface.ProbeType.Hid;
                }
                else if (tempInterface.Equals("FRIDGELOCK", StringComparison.OrdinalIgnoreCase))
                {
                    TemperatureInterface.ThermalProbeType = TemperatureInterface.ProbeType.FridgeLock;
                }
                else if (tempInterface.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
                {
                    TemperatureInterface.ThermalProbeType = TemperatureInterface.ProbeType.Rs232;
                    TemperatureInterface.ProbeComport = tempInterface;
                }

                // Init device
                TemperatureInterface.Init(tmpInterval == 0 ? 1000 : tmpInterval, temperature, temperatureIndex, temperatureOffset);

                // Report to local
                var tmpReportTime = 0;
                TemperatureInterface.OnTemperaturesReport = (temp) =>
                {
                    try
                    {
                        var tmpDto = new TemperatureDto(temp, temperatureIndex);
                        LogService.LogTemp($"Temperatures: {tmpDto}");
                        RabbitMqService.PublishTemperature(tmpDto);
                        StompService.PublishTemperature(tmpDto);
                        //Console.WriteLine($"Temperatures: {tmpDto}");
                        tmpReportTime += tmpInterval;
                        if (tmpReportTime >= tmpCloudInterval)
                        {
                            Task.Run(() =>
                            {
                                // Log to DB and report to cloud
                                TemperatureService.Log(tmpDto.Temp);
                            });

                            tmpReportTime = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogService.LogError(ex);
                    }
                };

                // Error handling
                TemperatureInterface.AlarmTemperatureAbnormal = (temp) =>
                {
                    var msg = RfidFridgeSetting.Alert.Messages.TemperatureIsAbnormal.Replace("{temp}", temp[temperatureIndex].ToString());
                    LogService.LogTemp(msg);
                    SlackService.SendAlert(MachineName, msg);
                };

                TemperatureInterface.OnError = (msg) =>
                {
                    SlackService.SendAlert(MachineName, msg);
                };
            }

            #endregion Temperature

            #region Machine Status

            MachineStatusService.Init(UseCloud);

            #endregion

            #region Camera

            Boolean.TryParse(RfidFridgeSetting.System.Camera.Enable, out bool cameraEnable);
            var sCameraEnable = (cameraEnable == true) ? "ENABLE" : "DISABLE";
            LogService.LogInfo($"Camera is {sCameraEnable}");
            if (cameraEnable)
            {
                int.TryParse(RfidFridgeSetting.System.Camera.CameraIndex, out int camIndex);
                int.TryParse(RfidFridgeSetting.System.Camera.ImageWidth, out int camImageW);
                int.TryParse(RfidFridgeSetting.System.Camera.ImageHeight, out int camImageH);
                var imagePath = RfidFridgeSetting.System.Camera.TransactionImagesFolder;
                CameraInterface.Init(camIndex, imagePath, camImageW, camImageH);
            }

            #endregion

            #region Blacklist
            bool.TryParse(RfidFridgeSetting.System.Payment.EnableBlackList, out bool enableBlacklist);
            BlacklistCardsService.Init(enableBlacklist);
            #endregion

            // LogService.LogInfo("POLLING 1>>>");

            var timerCheckDevicePolling = new System.Timers.Timer
            {
                Interval = 60 * 1000 * 5
            };
            timerCheckDevicePolling.Elapsed += TimerCheckDevicePolling_Elapsed;
            timerCheckDevicePolling.Start();
            // LogService.LogInfo("POLLING 2>>>");

            void TimerCheckDevicePolling_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                // LogService.LogInfo("POLLING>>>");

                if (!FridgeInterface.IsTransacting)
                {
                    var lastPolling = DateTime.Now;
                    foreach (var p in paymentTypes)
                    {
                        Enum.TryParse(p.ToUpper(), out Domain.Enums.PaymentType paymentType);
                        if (paymentType == PaymentType.MAGIC || paymentType == PaymentType.NAYAX)
                        {
                            if (paymentType == PaymentType.MAGIC)
                            {

                                if (RfidFridgeSetting.System.Payment.Magic.TerminalType == "IUC")
                                {
                                    lastPolling = FridgePayment.GetLastPolling();

                                    if (FridgeInterface.CurrentMachineStatus == MachineStatus.IDLE)
                                    {
                                        var isIucOk = FridgePayment.CheckDevice();
                                        if (!isIucOk)
                                        {
                                            LogService.LogInfo("IUC STOP WORKING");

                                            SlackService.SendAlert(RfidFridgeSetting.Machine.Name, "IUC STOP WORKING, SEND RESET COMMAND");
                                            FridgePayment.DisconnectHardware();
                                            Thread.Sleep(300);
                                            CmdExecuteService.ResetIuc(true);
                                            Thread.Sleep(300);
                                            FridgePayment.ReconnectHardware();
                                        }
                                        else
                                        {
                                            lastPolling = DateTime.Now;
                                            LogService.LogInfo("Iuc is working");
                                        }
                                    }

                                }

                                if (RfidFridgeSetting.System.Payment.Magic.TerminalType == "IM30")
                                {
                                    if (FridgeInterface.CurrentMachineStatus == MachineStatus.IDLE)
                                    {
                                        var isIucOk = FridgePayment.CheckDevice();
                                        if (!isIucOk)
                                        {
                                            LogService.LogInfo("IM30 STOP WORKING");

                                            SlackService.SendAlert(RfidFridgeSetting.Machine.Name, "IM30 STOP WORKING, SEND RESET COMMAND");
                                            FridgePayment.DisconnectHardware();
                                            Thread.Sleep(300);
                                            FridgePayment.ReconnectHardware();
                                        }
                                        else
                                        {
                                            lastPolling = DateTime.Now;
                                            LogService.LogInfo("IM30 is working");
                                        }
                                    }

                                }
                            }


                            if (paymentType == PaymentType.NAYAX)
                            {
                                lastPolling = NayaxInterface.GetLastPolling();
                            }

                            if (paymentType == PaymentType.PAYTER)
                            {
                                lastPolling = PayterInterface.GetLastPolling();
                            }

                            if ((DateTime.Now - lastPolling).TotalMinutes >= 10)
                            {
                                if (paymentType == PaymentType.MAGIC || paymentType == PaymentType.QR_MAGIC)
                                {

                                    LogService.LogInfo("CARD HOLDER STOP WORKING");
                                    SlackService.SendAlert(RfidFridgeSetting.Machine.Name, "CARD HOLDER STOP WORKING");


                                    bool.TryParse(RfidFridgeSetting.System.Payment.Magic.AutoEnableIfFailed, out bool settingAutoEnableIfFailed);
                                    if (settingAutoEnableIfFailed)
                                    {

                                        if (FridgeInterface.CurrentMachineStatus == MachineStatus.IDLE)
                                        {
                                            FridgePayment.Start();
                                            SlackService.SendAlert(RfidFridgeSetting.Machine.Name, "AUTO START PAYMENT DEVICE");
                                            LogService.LogMagicPaymentInfo("AUTO START PAYMENT DEVICE");
                                            CustomerUINotificationService.TapCard();
                                        }
                                        else
                                        {
                                            LogService.LogMagicPaymentInfo("Can't re-enable payment device, machine status is not IDLE | MachineStatus: " + FridgeInterface.CurrentMachineStatus);
                                        }
                                    }
                                }

                                if (paymentType == PaymentType.PAYTER)
                                {
                                    LogService.LogInfo("PAYTER STOP WORKING");
                                    SlackService.SendAlert(RfidFridgeSetting.Machine.Name, "PAYTER STOP WORKING");
                                }
                            }
                            else
                            {
                                LogService.LogInfo($"Payment device is working, last polling: {lastPolling} ");
                            }
                        }
                    }
                }

            }

            if (useCloud)
            {
                var timerRefreshCloudInventory = new System.Timers.Timer();
                timerRefreshCloudInventory.Interval = 60 * 1000 * 10;
                timerRefreshCloudInventory.Elapsed += RefreshCloudInventory;
                timerRefreshCloudInventory.Start();

                void RefreshCloudInventory(object sender, System.Timers.ElapsedEventArgs e)
                {
                    LogService.LogInfo("Timer | Report inventories: " + FridgeInterface.GetCurrentInventory().Count);
                    var d = FridgeInterface.GetCurrentInventory().Select(x => new KeyValuePair<string, string>(x.TagId, x.Product.ProductName));
                    var kv = new KeyValueMessage()
                    {
                        Key = MessageKeys.ProductRfidTagsRealtime,
                        MachineId = Guid.Parse(RfidFridgeSetting.Machine.Id),
                        Value = d
                    };

                    SendMessageToCloudService.SendMsgToCloud(kv);
                }
            }


            #region Test

            // FridgeInterface.CurrentMachineStatus = MachineStatus.IDLE;
            if (DeviceCheckingService.IsCheckingError())
            {
                FridgeInterface.CurrentMachineStatus = MachineStatus.DEVICE_CHECKING_ERROR;
                CustomerUINotificationService.PublishScreenBaseOnStatus(MachineStatus.DEVICE_CHECKING_ERROR);
                Thread.Sleep(30000);
                FridgeInterface.CurrentMachineStatus = MachineStatus.IDLE;
                CustomerUINotificationService.PublishScreenBaseOnStatus(MachineStatus.IDLE);
            }
            else
            {
                FridgeInterface.CurrentMachineStatus = MachineStatus.IDLE;
                CustomerUINotificationService.PublishScreenBaseOnStatus(MachineStatus.IDLE);
            }

            LogService.LogInfo("Init Done");


            while (true)
            {
                var text = Console.ReadLine();
                if (text == "recon")
                {
                    FridgeInterface.ReconnectInventory();
                }

                if (text == "cam")
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        CameraInterface.CaptureImage("TEST_CAPTURE_" + i);
                    }

                }
                if (text == "tmp")
                {
                    var random = new Random();
                    var tmp = random.Next(5, 10);
                    this.TemperatureService.Log((double)tmp);
                }

                if (text == "d")
                {
                    this.FridgeInterface.SetDebug();
                }


                if (text == "door")
                {
                    this.FridgeInterface.SetDebug();
                }

                if (text == "slack")
                {
                    this.SlackService.SendAlert("TEST_MACHINE", "TEST MESSAGE");
                }

                if (text == "mess")
                {
                    CustomerUINotificationService.OpenDoor();
                }

                if (text == "mess1")
                {
                    CustomerUINotificationService.InvalidCard(VALIDATE_ERROR_TYPE.EZLINK_BALANCE_NOT_ENOUGHT, 3210);
                }

                if (text == "mess2")
                {
                    CustomerUINotificationService.InvalidCard(VALIDATE_ERROR_TYPE.CANT_READ_CARD, 3210);
                }

                if (text == "pt")
                {
                    FridgeInterface.TestPublishTag_Web();
                }
                if (text == "po")
                {
                    FridgeInterface.TestPublishOrder();
                }
                if (text == "pi")
                {
                    FridgeInterface.TestPublishInventory();
                }

                if (text == "piw")
                {
                    FridgeInterface.TestPublishInventory_Web();
                }

                if (text == "open")
                {
                    FridgeInterface.OpenDoor();
                }
                if (text == "start")
                {
                    FridgeInterface.StartTransaction();
                }
                if (text == "tags")
                {
                    FridgeInterface.ListTagId2();
                }
                if (text == "0")
                {
                    FridgeInterface.CurrentMachineStatus = MachineStatus.IDLE;
                }
                if (text == "1")
                {
                    FridgeInterface.CurrentMachineStatus = MachineStatus.TRANSACTION_START;
                }
                if (text == "2")
                {
                    FridgeInterface.CurrentMachineStatus = MachineStatus.TRANSACTION_CONFIRM;
                }
                if (text == "3")
                {
                    FridgeInterface.CurrentMachineStatus = MachineStatus.TRANSACTION_DONE;
                }
                if (text == "4")
                {
                    FridgeInterface.CurrentMachineStatus = MachineStatus.STOPSALE;
                }
                if (text == "5")
                {
                    FridgeInterface.CurrentMachineStatus = MachineStatus.VALIDATE_CARD_FAILED;
                }
                if (text == "6")
                {
                    FridgeInterface.CurrentMachineStatus = MachineStatus.TRANSACTION_WAITTING_FOR_PAYMENT;
                }
                if (text == "7")
                {
                    FridgeInterface.CurrentMachineStatus = MachineStatus.STOPSALE_DUE_TO_PAYMENT;
                }
                if (text == "8")
                {
                    FridgeInterface.CurrentMachineStatus = MachineStatus.PREAUTH_MAKE_PAYMENT;
                }
                if (text == "9")
                {
                    FridgeInterface.CurrentMachineStatus = MachineStatus.PREAUTH_REFUND;
                }
            }

            #endregion Test


        }

        private void OnStpCmdRev(string cmd)
        {
            try
            {
                LogService.LogInfo("Stomp command received: " + cmd);
                var cmdArr = cmd.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
                var mainCmd = cmdArr[0];
                var subCmd = cmdArr[1];

                if (mainCmd == "FRID")
                {
                    if (subCmd == "OPEN")
                    {
                        FridgeInterface.OpenDoor();
                        // FridgeInterface.FakeDoorOpenStatus = false;
                    }
                }
                if (mainCmd == "TERMINAL")
                {
                    Enum.TryParse(RfidFridgeSetting.System.Payment.Type?.ToUpper(), out Domain.Enums.PaymentType paymentType);
                    if (subCmd == "ENABLE")
                    {
                        switch (paymentType)
                        {
                            case Domain.Enums.PaymentType.NAYAX:
                                this.NayaxInterface.EnableReader();
                                FridgeInterface.CurrentMachineStatus = MachineStatus.IDLE;
                                break;
                            case Domain.Enums.PaymentType.MAGIC:
                                FridgePayment.Start();
                                FridgeInterface.CurrentMachineStatus = MachineStatus.IDLE;
                                break;
                        }
                    }
                    if (subCmd == "DISABLE")
                    {
                        switch (paymentType)
                        {
                            case Domain.Enums.PaymentType.NAYAX:
                                this.NayaxInterface.DisableReader();
                                break;
                            case Domain.Enums.PaymentType.MAGIC:
                                FridgePayment.End();
                                break;
                        }
                    }

                    if (subCmd == "BL")
                    {
                        FridgePayment.SendCommand(TerminalCommand.IUC_BL);
                    }
                }
                if (mainCmd == "MACHINE")
                {
                    if (subCmd == "REFRESHPRD")
                    {
                        FridgeInterface.InitInventoryData();

                        if (UseCloud)
                        {
                            var inventories = FridgeInterface.GetCurrentInventory();
                            LogService.LogInfo("Report inventories: " + inventories.Count());
                            var d = inventories.Select(x => new KeyValuePair<string, string>(x.TagId, x.Product.ProductName));
                            var kv = new KeyValueMessage()
                            {
                                Key = MessageKeys.ProductRfidTagsRealtime,
                                MachineId = Guid.Parse(RfidFridgeSetting.Machine.Id),
                                Value = d
                            };

                            SendMessageToCloudService.SendMsgToCloud(kv);
                        }
                    }
                    if (subCmd == "REFRESHSETTINGS")
                    {
                        SettingService.GetAll();
                    }
                    if (subCmd == "STARTTXN")
                    {
                        FridgeInterface.IsTestTransaction = true;
                        FridgeInterface.StartTransaction();
                    }
                    if (subCmd == "REFRESHCLOUDINVENT")
                    {
                        var inventories = FridgeInterface.GetCurrentInventory();
                        LogService.LogInfo("Report inventories: " + inventories.Count());
                        var d = inventories.Select(x => new KeyValuePair<string, string>(x.TagId, x.Product.ProductName));
                        var kv = new KeyValueMessage()
                        {
                            Key = MessageKeys.ProductRfidTagsRealtime,
                            MachineId = Guid.Parse(RfidFridgeSetting.Machine.Id),
                            Value = d
                        };

                        SendMessageToCloudService.SendMsgToCloud(kv);
                    }
                }

                if (mainCmd == "CARDHOLDER")
                {
                    if (subCmd == "OPENLATCH")
                    {
                        FridgePayment.OpenLatch();
                    }
                }

                if (mainCmd == "CMD")
                {
                    if (subCmd == "CLEARUNSTABLERECORD")
                    {
                        UnstableTagService.ClearRecord();
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
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