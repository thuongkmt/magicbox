using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Domain;
using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.Enums;
using Konbini.RfidFridge.Service.Core;
using Konbini.RfidFridge.Service.Devices;
using Konbini.RfidFridge.Service.Util;
using MagicCashlessPayment.Core.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MagicCashlessPayment.Core.Devices.IucSerialPortInterfaceV2;

namespace MagicCashlessPayment.Core
{
    public class TestApplication : IFridgePayment
    {
        public CardReaderInterface Card;
        public IucSerialPortInterfaceV2 Iuc;
        public Action<PaymentType> OnValidateCardSuccess { get; set; }
        public Action OnValidateCardFailed { get; set; }
        public bool IsChargeFinished { get; set; }
        public string MachineName { get; set; }

        public CardPaymentType SelectedPaymentType { get; set; }
        private Thread _startThread;
        private LogService LogService;
        private SlackService SlackService;
        private CustomerUINotificationService CustomerUINotificationService;
        private int EzLinkBalance { get; set; }

        public TestApplication(LogService logService,
            CustomerUINotificationService customerUINotificationService,
            SlackService slackService
            )
        {
            LogService = logService;
            CustomerUINotificationService = customerUINotificationService;
            SlackService = slackService;
        }
        public bool Connect(string comport)
        {
            MachineName = RfidFridgeSetting.Machine.Name;


            //Card = new CardReaderInterface
            //{
            //    LogInfo = (x) => LogService.LogCardHolderInfo(x),
            //    LogHardware = (x) => LogService.LogCardHolderInfo(x),
            //    LogError = (x) => LogError(x)
            //};
            //var cardConnected = Card.Connect();
            //Card.Initialize(true);
            //Card.TurnOffAllLeds();
            //Card.LatchAutoLock(false);
            //Card.OpenLatch(true);

            Iuc = new IucSerialPortInterfaceV2(SlackService)
            {
                Debug = (x) => LogService.LogMagicPaymentInfo(x),
                Log = (x) => Console.WriteLine(x),
                LogInfo = (x) => LogService.LogInfo(x)
            };

            //Test();
            //return;

            var iucConnected = Iuc.ConnectPort(comport);
            //Iuc.CheckPortClose();
            return iucConnected;
        }

        public void Test(int amount, WalletLabel label)
        {
            Iuc.SendSaleCommand(IucSerialPortInterfaceV2.Commands.Settlement(),
                   (SaleApprove) =>
                   {
                       LogService.LogInfo("Settlement Approve");
                   },
                   (SaleError) =>
                   {
                       var response = (SaleResponse)SaleError;
                       LogService.LogInfo($"Settlement  Error: [{response.ResponseCode}] {response.Message}");
                   },
                   (SaleCancel) =>
                   {
                       LogService.LogInfo("Settlement Cancel");
                   },
                   (Callback) =>
                   {
                       LogService.LogInfo("Callback: " + Callback);
                   });
            //Iuc.Test();
            // Console.ReadLine();
            //Iuc.CheckCreditCard();
            //Iuc.SendSaleCommand(IucSerialPortInterfaceV2.Commands.Mpqr(amount, label),
            //       (SaleApprove) =>
            //       {
            //           LogService.LogInfo("Charge Mpqr Approve");
            //       },
            //       (SaleError) =>
            //       {
            //           var response = (SaleResponse)SaleError;
            //           LogService.LogInfo($"Charge Mpqr  Error: [{response.ResponseCode}] {response.Message}");
            //       },
            //       (SaleCancel) =>
            //       {
            //           LogService.LogInfo("Charge Mpqr Cancel");
            //       },
            //       (Callback) =>
            //       {
            //           LogService.LogInfo("Callback: " + Callback);
            //       });
        }

        private void LogError(string message)
        {
            LogService.LogInfo(message);
            SlackService?.SendAlert(MachineName, message);
        }

        private bool pollingFlag { get; set; }
        public void Start()
        {
            Card.CleanCommandQueue();
            //while (pollingFlag)
            //{
            //    pollingFlag = false;
            //    Thread.Sleep(1000);
            //    LogService.LogMagicPaymentInfo("Start thread still running, abort first");
            //}
            pollingFlag = true;
            _startThread?.Abort();
            _startThread = new Thread(DoWork);
            _startThread.Start();

            //Task.Run(() =>
            //{
            //    DoWork();
            //});
            void DoWork()
            {
                // 1. Wait for card insert
                var interval = 1000;

            START:
                LogService.LogMagicPaymentInfo("Magic Payment Start");

                Card.OpenLatch(true);
                Card.LatchAutoLock(false);
                Card.TurnOffAllLeds();
                Card.StartPoll(interval);

                Card.LedBlink(CardReaderInterface.PM_LED_TYPE.RED);
                // Check if card inside
                do
                {
                    var cardStatus = Card.Status;
                    if (cardStatus != null && cardStatus.IsCardInserted())
                    {
                    }
                    else
                    {
                        LogService.LogMagicPaymentInfo("Found existing card and removed");
                        Card.StopPoll();
                        break;
                    }
                    Thread.Sleep(interval);
                }
                while (pollingFlag);

                Card.LatchAutoLock(true);
                Card.TurnOffAllLeds();
                Card.LedBlink(CardReaderInterface.PM_LED_TYPE.GREEN);
                Card.StartPoll(interval);

                // Waiting for card insert
                do
                {
                    var cardStatus = Card.Status;
                    if (cardStatus != null && cardStatus.IsCardInserted())
                    {
                        // Card inserted
                        var message = "Card inserted!!!";
                        LogService.LogMagicPaymentInfo(message);
                        SlackService.SendInfo(MachineName, message);
                        break;
                    }
                    Thread.Sleep(interval);
                }
                while (pollingFlag);

                VALIDATE_ERROR_TYPE error = VALIDATE_ERROR_TYPE.NONE;
                int ezLinkAmount = 0;
                // Validate
                if (Validate(ref error, ref ezLinkAmount))
                {
                    // Lock the card
                    LogService.LogInfo("Lock LATCH after validate");
                    Card.OpenLatch(false);
                    do
                    {
                        var cardStatus = Card.Status;
                        if (cardStatus != null && cardStatus.IsLatchLock())
                        {
                            // Latch lock
                            LogService.LogMagicPaymentInfo("Latch locked!!!");
                            OnValidateCardSuccess?.Invoke(PaymentType.MAGIC);
                            Card.LedSolid(CardReaderInterface.PM_LED_TYPE.GREEN, CardReaderInterface.PM_LED_STATE.OFF);
                            Card.LedSolid(CardReaderInterface.PM_LED_TYPE.GREEN, CardReaderInterface.PM_LED_STATE.ON);

                            Card.StopPoll();

                            break;
                        }
                        Thread.Sleep(interval);
                    }
                    while (pollingFlag);
                }
                else
                {
                    LogService.LogMagicPaymentInfo("Balance not valid, remove your card");
                    SlackService.SendInfo(MachineName, "Card is not valid");
                    LogService.LogInfo("OnValidateCardFailed: " + (OnValidateCardFailed == null));

                    //OnValidateCardFailed?.Invoke();
                    CustomerUINotificationService.InvalidCard(error, ezLinkAmount);
                    do
                    {
                        if (Card.Status != null && Card.Status.IsLatchLock())
                        {
                            // Latch open
                            LogService.LogInfo("OPEN If not valid");
                            Card.OpenLatch(true);
                            LogService.LogMagicPaymentInfo("Latch released!!!");
                            Card.LedSolid(CardReaderInterface.PM_LED_TYPE.GREEN, CardReaderInterface.PM_LED_STATE.OFF);

                            Card.LedBlink(CardReaderInterface.PM_LED_TYPE.RED);
                            //Thread.Sleep(2000);

                            while (Card.Status.IsCardInserted())
                            {
                                Thread.Sleep(interval);
                            }
                            CustomerUINotificationService.TapCard();
                            Thread.Sleep(1000);
                            goto START;
                        }

                        Thread.Sleep(interval);
                    }
                    while (pollingFlag);
                }
            }

        }
        public bool Validate(ref VALIDATE_ERROR_TYPE error, ref int ezLinkBalance)
        {
            EzLinkBalance = 0;
            Card.CleanCommandQueue();
            LogService.LogMagicPaymentInfo("Validating");
            CustomerUINotificationService.ValidateCard();
            int.TryParse(RfidFridgeSetting.System.Payment.Magic.MinBalanceRequire, out int minBalance);
            bool.TryParse(RfidFridgeSetting.System.Payment.Magic.EnableCreditCard, out bool enCreditCard);

            var cnumber = string.Empty;
            var balance = Iuc.CheckCpasBalance(ref cnumber);
            ezLinkBalance = balance;
            Thread.Sleep(1000);
            var result = false;
            if (minBalance == 0)
            {
                minBalance = 20000;
            }
            if (balance >= minBalance)
            {
                // CPAS
                EzLinkBalance = balance;
                SelectedPaymentType = CardPaymentType.CPAS;
                LogService.LogMagicPaymentInfo($"Selected Payment: " + SelectedPaymentType);
                SlackService.SendInfo(MachineName, $"Selected Payment: " + SelectedPaymentType);
                result = true;
            }
            else if (balance < minBalance)
            {
                error = VALIDATE_ERROR_TYPE.EZLINK_BALANCE_NOT_ENOUGHT;
                SelectedPaymentType = CardPaymentType.NONE;
                result = false;
            }
            else
            {
                error = VALIDATE_ERROR_TYPE.CANT_READ_CARD;
                SelectedPaymentType = CardPaymentType.NONE;
                result = false;
            }

            if (enCreditCard)
            {
                if (Iuc.CheckCreditCard())
                {
                    SelectedPaymentType = CardPaymentType.CREDITCARD;
                    LogService.LogMagicPaymentInfo($"Selected Payment: " + SelectedPaymentType);
                    SlackService.SendInfo(MachineName, $"Selected Payment: " + SelectedPaymentType);
                    result = true;
                }
                else
                {
                    error = VALIDATE_ERROR_TYPE.CANT_READ_CARD;
                    SelectedPaymentType = CardPaymentType.NONE;
                    result = false;
                }
            }
            else
            {
                LogService.LogMagicPaymentInfo($"Credit card is not enable, skip checking");
            }

            var message = $"Validate result: {result} | {error}";
            LogService.LogMagicPaymentInfo(message);

            return result;
        }

        public void Charge(int amount, Action<TransactionStatus, object, CardPaymentType> callback = null)
        {
            IsChargeFinished = false;

            Card.StopPoll();
            Card.CleanCommandQueue();

            LogService.LogMagicPaymentInfo($"Charge: {amount} | Payment mode: {SelectedPaymentType}");
            SlackService.SendInfo(MachineName, $"Charge: {amount} | Payment mode: {SelectedPaymentType}");

            Thread.Sleep(200);

            if (SelectedPaymentType == CardPaymentType.CPAS)
            {
                LogService.LogMagicPaymentInfo($"EzLink Balance: {EzLinkBalance}");
                if (amount > EzLinkBalance)
                {
                    amount = EzLinkBalance;
                }

                Iuc.SendSaleCommand(IucSerialPortInterface.Commands.Cpas(amount),
                    (SaleApprove) =>
                    {
                        LogService.LogMagicPaymentInfo("Charge CPAS Approve");

                        End();
                        callback?.Invoke(TransactionStatus.Success, SaleApprove, CardPaymentType.CPAS);
                        SlackService.SendInfo(MachineName, "Charge CPAS Approve");
                        IsChargeFinished = true;
                    },
                    (SaleError) =>
                    {
                        var response = (SaleResponse)SaleError;
                        LogService.LogMagicPaymentInfo($"Charge CPAS  Error: [{response.ResponseCode}] {response.Message}");

                        End();
                        callback?.Invoke(TransactionStatus.Error, SaleError, CardPaymentType.CPAS);
                        SlackService.SendInfo(MachineName, $"Charge CPAS  Error: [{response.ResponseCode}] {response.Message}");
                        IsChargeFinished = true;
                    },
                    (SaleCancel) =>
                    {
                        LogService.LogMagicPaymentInfo("Charge CPAS Cancel");

                        End();
                        callback?.Invoke(TransactionStatus.Error, SaleCancel, CardPaymentType.CPAS);

                        SlackService.SendInfo(MachineName, "Charge CPAS Cancel");
                        IsChargeFinished = true;
                    },
                    (Callback) =>
                    {
                        LogService.LogMagicPaymentInfo("Charge Callback: " + Callback);
                    });
            }
            if (SelectedPaymentType == CardPaymentType.CREDITCARD)
            {
                Iuc.SendSaleCommand(IucSerialPortInterface.Commands.Sale(amount),
                    (SaleApprove) =>
                    {
                        LogService.LogMagicPaymentInfo("Charge CC Approve");
                        End();
                        callback?.Invoke(TransactionStatus.Success, SaleApprove, CardPaymentType.CREDITCARD);
                        SlackService.SendInfo(MachineName, "Charge CC Approve");
                        IsChargeFinished = true;
                    },
                    (SaleError) =>
                    {
                        var response = (SaleResponse)SaleError;
                        LogService.LogMagicPaymentInfo($"Charge CC Error: [{response.ResponseCode}] {response.Message}");
                        End();
                        callback?.Invoke(TransactionStatus.Error, SaleError, CardPaymentType.CREDITCARD);
                        SlackService.SendInfo(MachineName, $"Charge CC Error: [{response.ResponseCode}] {response.Message}");
                        IsChargeFinished = true;
                    },
                    (SaleCancel) =>
                    {
                        LogService.LogMagicPaymentInfo("Charge CC Cancel");
                        End();
                        callback?.Invoke(TransactionStatus.Error, SaleCancel, CardPaymentType.CREDITCARD);
                        SlackService.SendInfo(MachineName, "Charge CC Cancel");
                        IsChargeFinished = true;
                    },
                    (Callback) =>
                    {
                        LogService.LogMagicPaymentInfo("Charge CC Callback: " + Callback);
                    });
            }

            // LogService.LogMagicPaymentInfo("Charge done!!");
        }

        public void End(bool openLatch = true)
        {
            pollingFlag = false;
            LogService.LogMagicPaymentInfo($"Magic Payment End | Open Latch: {openLatch}");

            // Make sure everything stop
            Thread.Sleep(1000);
            _startThread?.Abort();

            Card.StopPoll();
            Card.CleanCommandQueue();

            Card.OpenLatch(openLatch);
            Card.LatchAutoLock(false);

            Iuc.CancelCommand();
            Card.TurnOffAllLeds();

            Card.LedBlink(CardReaderInterface.PM_LED_TYPE.RED);
        }

        public void OpenLatch()
        {
            Card.OpenLatch(true);
        }

        public DateTime GetLastPolling()
        {
            return Card.LastPoll;
        }

        public bool CheckDevice()
        {
            throw new NotImplementedException();
        }

        public void Charge(int amount, Action<TransactionStatus, object, CardPaymentType, string> callback = null)
        {
            throw new NotImplementedException();
        }

        public object SendCommand(TerminalCommand cmd)
        {
            throw new NotImplementedException();
        }

        public void DisconnectHardware()
        {
            throw new NotImplementedException();
        }

        public void ReconnectHardware()
        {
            throw new NotImplementedException();
        }

        public void CustomerAction(CustomerAction action)
        {
            throw new NotImplementedException();
        }

        public void Refund(Action<bool> isSuccess)
        {
            throw new NotImplementedException();
        }
    }
}
