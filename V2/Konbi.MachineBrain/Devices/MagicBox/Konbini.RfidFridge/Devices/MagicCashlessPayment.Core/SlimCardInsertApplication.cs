using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Domain;
using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.Enums;
using Konbini.RfidFridge.Domain.Enums.DeviceChecking;
using Konbini.RfidFridge.Service.Core;
using Konbini.RfidFridge.Service.Data;
using Konbini.RfidFridge.Service.Devices;
using Konbini.RfidFridge.Service.Util;
using MagicCashlessPayment.Core.Devices;
using MagicCashlessPayment.Core.Devices.SlimCardInsert;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MagicCashlessPayment.Core.Devices.R2Interface.Response;

namespace MagicCashlessPayment.Core
{
    public class SlimCardInsertApplication : IFridgePayment
    {
        public SlimCardReaderInterface Card;

        //public IucSerialPortInterface Iuc;
        public IucSerialPortInterfaceV2 Iuc;
        public R2Interface R2Terminal;

        public Action<PaymentType> OnValidateCardSuccess { get; set; }
        public Action OnValidateCardFailed { get; set; }
        public bool IsChargeFinished { get; set; }
        public string MachineName { get; set; }

        public CardPaymentType SelectedPaymentType { get; set; }
        private Thread _startThread;
        private LogService LogService;
        private SlackService SlackService;
        private CustomerUINotificationService CustomerUINotificationService;
        private IBlacklistCardsService BlacklistCardsService;
        private DeviceCheckingService DeviceCheckingService;

        private int EzLinkBalance { get; set; }
        private string CpasCardNumber { get; set; }
        int errorRetryChargeTime = 0;
        private MagicPaymentTerminalType TerminalType { get; set; }

        public SlimCardInsertApplication(LogService logService,
            CustomerUINotificationService customerUINotificationService,
            SlackService slackService,
            IBlacklistCardsService blacklistCardsService,
            DeviceCheckingService deviceCheckingService
            )
        {
            LogService = logService;
            CustomerUINotificationService = customerUINotificationService;
            SlackService = slackService;
            BlacklistCardsService = blacklistCardsService;
            DeviceCheckingService = deviceCheckingService;
        }
        public bool Connect(string comport)
        {
            try
            {
                LogService.LogInfo("Using Slim Card Insert Application");

          


                MachineName = RfidFridgeSetting.Machine.Name;
                Enum.TryParse(RfidFridgeSetting.System.Payment.Magic.TerminalType, out MagicPaymentTerminalType terinalType);
                Enum.TryParse(RfidFridgeSetting.System.Payment.Magic.CardInsertType, out MagicPaymentCardInsertType cardInsertType);

                TerminalType = terinalType;
                LogService.LogInfo("Magic Payment Terminal Type: " + TerminalType.ToString());
                LogService.LogInfo("Magic Payment Card Insert Type: " + cardInsertType.ToString());

                var cardInsertCom = RfidFridgeSetting.System.Payment.Magic.CardInsertComport;

                DeviceCheckingService.AddToChecklist(Konbini.RfidFridge.Domain.Enums.DeviceChecking.DeviceName.PAYMENT_TERMINAL, comport);
                DeviceCheckingService.AddToChecklist(Konbini.RfidFridge.Domain.Enums.DeviceChecking.DeviceName.CARD_INSERT, cardInsertCom);

                if (cardInsertType == MagicPaymentCardInsertType.SLIM)
                {
                    LogService.LogInfo("Magic Payment Card Insert Comport: " + cardInsertCom);
                }
                Card = new SlimCardReaderInterface
                {
                    LogInfo = (x) => LogService.LogCardHolderInfo(x),
                    LogHardware = (x) => LogService.LogCardHolderInfo(x),
                    LogError = (x) => LogError(x)
                };
                Card.DebugMode = true;
                var cardConnected = Card.ConnectPort(cardInsertCom);
                //Card.Initialize(true);
                //Card.TurnOffAllLeds();
                // Card.LatchAutoLock(false);
                Thread.Sleep(1000); 
                Card.CurrentStatus((success) =>
                {
                    DeviceCheckingService.UpdateStatus(DeviceName.CARD_INSERT, DeviceStatus.OK);
                    LogService.LogInfo("Card Insert is Working");
                }, (error) =>
                {
                    DeviceCheckingService.UpdateStatus(DeviceName.CARD_INSERT, DeviceStatus.ERROR);
                    LogService.LogError("Card Insert is NOT Working");
                    SlackService.SendAlert(MachineName, "Card Insert is NOT Working");
                });
                Thread.Sleep(1000);

                Card.OpenLatch(true);


                var terminalConnected = false;

                if (TerminalType == MagicPaymentTerminalType.IUC)
                {
                    Iuc = new IucSerialPortInterfaceV2(SlackService)
                    {
                        Debug = (x) => LogService.LogMagicPaymentInfo(x),
                        Log = (x) => LogService.LogTerminalInfo(x),
                        LogInfo = (x) => LogService.LogInfo(x),
                        LogError = (x) => LogService.LogError(x)
                    };
                    terminalConnected = Iuc.ConnectPort(comport);
                    if(terminalConnected)
                    {
                        DeviceCheckingService.UpdateStatus(DeviceName.PAYMENT_TERMINAL, DeviceStatus.OK);
                    }
                    else
                    {
                        DeviceCheckingService.UpdateStatus(DeviceName.PAYMENT_TERMINAL, DeviceStatus.ERROR);
                    }
                    Iuc.CheckPortClose();
                }
                if (TerminalType == MagicPaymentTerminalType.R2)
                {
                    R2Terminal = new R2Interface(SlackService)
                    {
                        Log = (x) => LogService.LogTerminalInfo(x),
                        LogInfo = (x) => LogService.LogInfo(x)
                    };
                    R2Terminal.Connect(comport);
                }

                return terminalConnected && cardConnected;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
                return false;
            }
        }

        private void LogError(string message)
        {
            LogService.LogInfo(message);
            SlackService.SendAlert(MachineName, message);
        }

        private bool pollingFlag { get; set; }
        List<string> iucErrorCodes = new List<string>();

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
            iucErrorCodes = RfidFridgeSetting.System.Payment.Magic.RetryToChargeIfGotErrorCodes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

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
                errorRetryChargeTime = 0;

                Card.OpenLatch(true);
                //Card.LatchAutoLock(false);
                //Card.TurnOffAllLeds();
                Card.StartPoll(interval);

                // Card.LedBlink(CardReaderInterface.PM_LED_TYPE.RED);
                // Check if card inside
                do
                {
                    var cardStatus = Card.Status;
                    if (cardStatus != null && cardStatus.IsCardInserted())
                    {
                    }
                    else
                    {
                        //LogService.LogMagicPaymentInfo("Found existing card and removed");
                        Card.StopPoll();
                        break;
                    }
                    Thread.Sleep(interval);
                }
                while (pollingFlag);

                // Card.LatchAutoLock(true);
                // Card.TurnOffAllLeds();
                // Card.LedBlink(CardReaderInterface.PM_LED_TYPE.GREEN);
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

                if (!pollingFlag)
                {
                    return;
                }

                VALIDATE_ERROR_TYPE error = VALIDATE_ERROR_TYPE.NONE;
                int ezLinkAmount = 0;
                string errorMessage = string.Empty;
                // Validate
                if (Validate(ref error, ref ezLinkAmount, ref errorMessage))
                {
                    // Lock the card
                    //LogService.LogInfo("Lock LATCH after validate");
                    Card.OpenLatch(false);
                    do
                    {
                        var cardStatus = Card.Status;
                        if (cardStatus != null && cardStatus.IsLatchLock())
                        {
                            // Latch lock
                            LogService.LogMagicPaymentInfo("Latch locked!!!");
                            OnValidateCardSuccess?.Invoke(PaymentType.MAGIC);
                            // Card.LedSolid(CardReaderInterface.PM_LED_TYPE.GREEN, CardReaderInterface.PM_LED_STATE.OFF);
                            // Card.LedSolid(CardReaderInterface.PM_LED_TYPE.GREEN, CardReaderInterface.PM_LED_STATE.ON);

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
                    CustomerUINotificationService.InvalidCard(error, ezLinkAmount, errorMessage);
                    do
                    {
                        if (Card.Status != null && Card.Status.IsLatchLock())
                        {
                            // Latch open
                            LogService.LogInfo("OPEN If not valid");
                            Card.OpenLatch(true);
                            LogService.LogMagicPaymentInfo("Latch released!!!");
                            // Card.LedSolid(CardReaderInterface.PM_LED_TYPE.GREEN, CardReaderInterface.PM_LED_STATE.OFF);

                            //  Card.LedBlink(CardReaderInterface.PM_LED_TYPE.RED);
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
        public bool Validate(ref VALIDATE_ERROR_TYPE error, ref int ezLinkBalance, ref string errorMessage)
        {
            EzLinkBalance = 0;
            Card.CleanCommandQueue();
            var result = false;

            LogService.LogMagicPaymentInfo("Validating");
            CustomerUINotificationService.ValidateCard();
            int.TryParse(RfidFridgeSetting.System.Payment.Magic.MinBalanceRequire, out int minBalance);
            bool.TryParse(RfidFridgeSetting.System.Payment.Magic.EnableCreditCard, out bool enCreditCard);
            bool.TryParse(RfidFridgeSetting.System.Payment.Magic.EnableCpas, out bool enCpas);
            LogService.LogMagicPaymentInfo($"CPAS enable = {enCpas} | Creadit Card enable = {enCreditCard}");

            switch (TerminalType)
            {
                case MagicPaymentTerminalType.IUC:
                    Iuc.ClearResponseQueue();
                    if(enCpas)
                    {
                        var cardNumber = string.Empty;
                        var balance = Iuc.CheckCpasBalance(ref cardNumber);
                        ezLinkBalance = balance;
                        Thread.Sleep(1000);

                        if (!string.IsNullOrEmpty(cardNumber))
                        {
                            // Check blacklist first
                            var isBlacklist = BlacklistCardsService.CheckCPASBlaskList(cardNumber);
                            CpasCardNumber = cardNumber;
                            if (isBlacklist)
                            {
                                error = VALIDATE_ERROR_TYPE.EZLINK_BLACKLIST;
                                SelectedPaymentType = CardPaymentType.NONE;
                                result = false;

                                if (!enCreditCard)
                                    CustomerUINotificationService.CardIsBlacklist();
                            }
                            else
                            {
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
                                    return true;
                                }
                                else if (balance < minBalance)
                                {
                                    error = VALIDATE_ERROR_TYPE.EZLINK_BALANCE_NOT_ENOUGHT;
                                    SelectedPaymentType = CardPaymentType.NONE;
                                    result = false;
                                }
                            }
                        }
                        else
                        {
                            error = VALIDATE_ERROR_TYPE.CANT_READ_CARD;
                            SelectedPaymentType = CardPaymentType.NONE;
                            result = false;
                        }
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
                    break;
                case MagicPaymentTerminalType.R2:
                    CARD_LABEL cardLabel = CARD_LABEL.NONE;
                    var cancelMessage = string.Empty;
                    var isValid = R2Terminal.Validate(minBalance, ref cardLabel, ref cancelMessage);


                    errorMessage = cancelMessage;

                    if (isValid)
                    {
                        if (cardLabel == CARD_LABEL.CONCESSION || cardLabel == CARD_LABEL.EZLINK || cardLabel == CARD_LABEL.NFP)
                        {
                            SelectedPaymentType = CardPaymentType.CPAS;
                        }
                        else if (cardLabel == CARD_LABEL.MASTER || cardLabel == CARD_LABEL.VISA)
                        {
                            SelectedPaymentType = CardPaymentType.CREDITCARD;
                        }

                        LogService.LogMagicPaymentInfo($"Selected Payment: " + SelectedPaymentType);
                        SlackService.SendInfo(MachineName, $"Selected Payment: " + SelectedPaymentType);
                        result = true;
                    }
                    else
                    {
                        // Rewrite message based on error code
                        if (errorMessage.StartsWith("[21]") || errorMessage.StartsWith("[15]"))
                        {
                            errorMessage = "INSUFFICIENT BALANCE<br>MINIMUM BALANCE REQUIRE: {min}";
                            var tmp = errorMessage;
                            Task.Run(() =>
                            {
                                SlackService.SendAlert(MachineName, $"Validate card error: " + tmp);
                            });
                        }
                        else
                        {
                            Task.Run(() =>
                            {
                                SlackService.SendAlert(MachineName, $"Validate card error: " + cancelMessage);
                            });
                        }
                        error = VALIDATE_ERROR_TYPE.GENERAL_ERROR;
                        SelectedPaymentType = CardPaymentType.NONE;
                        result = false;
                    }
                    break;
            }



            var message = $"Validate result: {result} | {error}";
            LogService.LogMagicPaymentInfo(message);

            return result;
        }

        public void Charge(int amount, Action<TransactionStatus, object, CardPaymentType, string> callback = null)
        {
            IsChargeFinished = false;

            Card.StopPoll();
            Card.CleanCommandQueue();


            LogService.LogMagicPaymentInfo($"Charge: {amount} | Payment mode: {SelectedPaymentType}");
            SlackService.SendInfo(MachineName, $"Charge: {amount} | Payment mode: {SelectedPaymentType}");

            Thread.Sleep(200);

            switch (TerminalType)
            {
                case MagicPaymentTerminalType.IUC:
                    #region IUC
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
                                callback?.Invoke(TransactionStatus.Success, SaleApprove, CardPaymentType.CPAS, CpasCardNumber);
                                SlackService.SendInfo(MachineName, "Charge CPAS Approve");
                                IsChargeFinished = true;
                            },
                            (SaleError) =>
                            {
                                var response = (SaleResponse)SaleError;
                                LogService.LogMagicPaymentInfo($"Charge CPAS  Error: [{response.ResponseCode}] {response.Message}");

                                if (iucErrorCodes.Any(x => x.Equals(response.ResponseCode)))
                                {
                                    if (++errorRetryChargeTime <= 1)
                                    {
                                        LogService.LogInfo($"Retring to charge...");
                                        SlackService.SendAlert(RfidFridgeSetting.Machine.Name, $"Found error code {response.ResponseCode}, retring to charge...");
                                        Charge(amount, callback);
                                        return;
                                    }
                                }

                                End();
                                callback?.Invoke(TransactionStatus.Error, SaleError, CardPaymentType.CPAS, CpasCardNumber);
                                SlackService.SendInfo(MachineName, $"Charge CPAS  Error: [{response.ResponseCode}] {response.Message}");
                                IsChargeFinished = true;
                            },
                            (SaleCancel) =>
                            {
                                LogService.LogMagicPaymentInfo("Charge CPAS Cancel");
                                End();
                                callback?.Invoke(TransactionStatus.Error, SaleCancel, CardPaymentType.CPAS, CpasCardNumber);
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
                                callback?.Invoke(TransactionStatus.Success, SaleApprove, CardPaymentType.CREDITCARD, string.Empty);
                                SlackService.SendInfo(MachineName, "Charge CC Approve");
                                IsChargeFinished = true;
                            },
                            (SaleError) =>
                            {
                                var response = (SaleResponse)SaleError;
                                LogService.LogMagicPaymentInfo($"Charge CC Error: [{response.ResponseCode}] {response.Message}");

                                if (iucErrorCodes.Any(x => x.Equals(response.ResponseCode)))
                                {
                                    if (++errorRetryChargeTime <= 1)
                                    {
                                        LogService.LogInfo($"Retring to charge...");
                                        SlackService.SendAlert(RfidFridgeSetting.Machine.Name, $"Found error code {response.ResponseCode}, retring to charge...");
                                        Charge(amount, callback);
                                    }
                                }

                                End();
                                callback?.Invoke(TransactionStatus.Error, SaleError, CardPaymentType.CREDITCARD, string.Empty);
                                SlackService.SendInfo(MachineName, $"Charge CC Error: [{response.ResponseCode}] {response.Message}");
                                IsChargeFinished = true;
                            },
                            (SaleCancel) =>
                            {
                                LogService.LogMagicPaymentInfo("Charge CC Cancel");
                                End();
                                callback?.Invoke(TransactionStatus.Error, SaleCancel, CardPaymentType.CREDITCARD, string.Empty);
                                SlackService.SendInfo(MachineName, "Charge CC Cancel");
                                IsChargeFinished = true;
                            },
                            (Callback) =>
                            {
                                LogService.LogMagicPaymentInfo("Charge CC Callback: " + Callback);
                            });
                    }

                    #endregion
                    break;
                case MagicPaymentTerminalType.R2:
                    var tryTime = 0;
                    var amountToCharge = amount;
                RETRY_TO_CHARGE:

                    R2Interface.Response r2response = null;
                    var isSuccess = R2Terminal.Charge(amountToCharge, ref r2response);
                    LogService.LogInfo("Charge Response: " + JsonConvert.SerializeObject(r2response));
                    int.TryParse(RfidFridgeSetting.System.Payment.Magic.MinBalanceRequire, out int minBalance);

                    if (isSuccess)
                    {
                        // need to convert to IUCreponse
                        var saleApproveResponse = new IucApprovedResponse();
                        try
                        {
                            saleApproveResponse = new IucApprovedResponse()
                            {
                                Amount = amountToCharge / 100m,
                                ApproveCode = r2response.ApproveCode,
                                CardLabel = r2response.CardLabel.ToString(),
                                CardNumber = r2response.CardNumer
                            };
                            LogService.LogMagicPaymentInfo("Charge Approve");
                        }
                        catch (Exception ex)
                        {
                            LogService.LogError(ex);
                        }

                        End();
                        callback?.Invoke(TransactionStatus.Success, saleApproveResponse, SelectedPaymentType, string.Empty);
                        SlackService.SendInfo(MachineName, "Charge Approve");
                        IsChargeFinished = true;
                    }
                    else
                    {
                        var saleErrorReponse = new SaleResponse();
                        try
                        {
                            saleErrorReponse = new SaleResponse()
                            {
                                Amout = amountToCharge,
                                ResponseCode = r2response.ResponseCode,
                                Message = R2Interface.Response.GetResponseCode(r2response.ResponseCode)
                            };
                            LogService.LogMagicPaymentInfo($"Charge Error: [{saleErrorReponse.ResponseCode}] {saleErrorReponse.Message}");

                            if (r2response.ResponseCode == "D2")
                            {
                                if (++tryTime <= 1)
                                {
                                    SlackService.SendInfo(MachineName, $"Got D2 error, retry to charge Reserve Amount");
                                    amountToCharge = minBalance;
                                    goto RETRY_TO_CHARGE;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogService.LogError(ex);
                        }

                        End();
                        callback?.Invoke(TransactionStatus.Error, saleErrorReponse, SelectedPaymentType, string.Empty);
                        SlackService.SendInfo(MachineName, $"Charge Error: [{saleErrorReponse.ResponseCode}] {saleErrorReponse.Message}");
                        IsChargeFinished = true;
                    }
                    break;
            }



        }

        public void End(bool openLatch = true)
        {
            pollingFlag = false;
            _startThread?.Abort();

            LogService.LogMagicPaymentInfo($"Magic Payment End | Open Latch: {openLatch}");


            // Make sure everything stop
            Thread.Sleep(1000);

            Card.StopPoll();
            Card.CleanCommandQueue();

            Card.OpenLatch(openLatch);
            //Card.LatchAutoLock(false);

            Iuc?.CancelCommand();
            // Card.TurnOffAllLeds();

            // Card.LedBlink(CardReaderInterface.PM_LED_TYPE.RED);
        }

        public object SendCommand(TerminalCommand cmd)
        {
            object result = null;
            switch (cmd)
            {
                case TerminalCommand.IUC_BL:
                    result = Iuc.CheckBlacklist();
                    break;
            }
            return result;
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
            return Iuc.CheckDevice();
        }

        public void DisconnectHardware()
        {
            Iuc.Disconnect();
        }

        public void ReconnectHardware()
        {
            Iuc.ConnectPort(Iuc.ComportName);
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
