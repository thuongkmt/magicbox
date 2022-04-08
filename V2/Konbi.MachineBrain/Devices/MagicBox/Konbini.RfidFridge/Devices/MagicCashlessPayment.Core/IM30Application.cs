using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Domain;
using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.Enums;
using Konbini.RfidFridge.Service.Core;
using Konbini.RfidFridge.Service.Data;
using Konbini.RfidFridge.Service.Devices;
using Konbini.RfidFridge.Service.Util;
using MagicCashlessPayment.Core.Devices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MagicCashlessPayment.Core
{
    public class IM30Application : IFridgePayment
    {

        //public IucSerialPortInterface Iuc;
        public IucSerialPortInterfaceV2 Iuc;

        public Action<PaymentType> OnValidateCardSuccess { get; set; }
        public Action OnValidateCardFailed { get; set; }
        public bool IsChargeFinished { get; set; }
        public string MachineName { get; set; }

        private LogService LogService;
        private SlackService SlackService;
        private CustomerUINotificationService CustomerUINotificationService;
        private DeviceCheckingService DeviceCheckingService;
        private IucApprovedResponse CurrentPreauthResponse = new IucApprovedResponse();

        private MagicPaymentTerminalType TerminalType { get; set; }

        public IM30Application(LogService logService,
            CustomerUINotificationService customerUINotificationService,
            SlackService slackService,
            DeviceCheckingService deviceCheckingService
            )
        {
            LogService = logService;
            CustomerUINotificationService = customerUINotificationService;
            SlackService = slackService;
            DeviceCheckingService = deviceCheckingService;
        }
        public bool Connect(string comport)
        {
            try
            {
                LogService.LogInfo("Using Default IM30 Terminal Application");
                DeviceCheckingService.AddToChecklist(Konbini.RfidFridge.Domain.Enums.DeviceChecking.DeviceName.PAYMENT_TERMINAL, comport);

                MachineName = RfidFridgeSetting.Machine.Name;
                Enum.TryParse(RfidFridgeSetting.System.Payment.Magic.TerminalType, out MagicPaymentTerminalType terinalType);
                TerminalType = terinalType;
                LogService.LogInfo("Terminal Type: " + TerminalType.ToString());
                DeviceCheckingService.UpdateFriendlyName(Konbini.RfidFridge.Domain.Enums.DeviceChecking.DeviceName.PAYMENT_TERMINAL, TerminalType.ToString());

                var terminalConnected = false;

                Iuc = new IucSerialPortInterfaceV2(SlackService)
                {
                    Debug = (x) => LogService.LogMagicPaymentInfo(x),
                    Log = (x) => LogService.LogTerminalInfo(x),
                    LogInfo = (x) => LogService.LogInfo(x),
                    LogError = (x) => LogService.LogError(x)

                };
                terminalConnected = Iuc.ConnectPort(comport);
                Iuc.CheckPortClose();

                if(terminalConnected)
                {
                    Iuc.GetTidAndMid();
                }
        
                var status = (terminalConnected && !string.IsNullOrEmpty(Iuc.Tid)) ? Konbini.RfidFridge.Domain.Enums.DeviceChecking.DeviceStatus.OK : Konbini.RfidFridge.Domain.Enums.DeviceChecking.DeviceStatus.ERROR;
                DeviceCheckingService.UpdateStatus(Konbini.RfidFridge.Domain.Enums.DeviceChecking.DeviceName.PAYMENT_TERMINAL, status);


                return terminalConnected;
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


        }

        public void CustomerAction(CustomerAction action)
        {
            switch (action)
            {
                case Konbini.RfidFridge.Domain.Enums.CustomerAction.PRESS_START_BUTTON:
                    SlackService.SendInfo(MachineName, $"Customer Pressed Start Button");

                    if (Validate())
                    {
                        OnValidateCardSuccess?.Invoke(PaymentType.MAGIC);
                    }
                    break;
            }
        }

        bool isValidating = false;
        public bool Validate()
        {

            if (isValidating)
            {
                LogService.LogMagicPaymentInfo("Validating Card is in process | Skip");
                return false;
            }
            isValidating = true;
            var result = false;
            var isCmdFinished = false;

            LogService.LogMagicPaymentInfo("Validating Card");
            CustomerUINotificationService.ValidateCard();
            int.TryParse(RfidFridgeSetting.System.Payment.Magic.MinBalanceRequire, out int minBalance);
            bool.TryParse(RfidFridgeSetting.System.Payment.Magic.EnableCreditCard, out bool enCreditCard);
            SlackService.SendInfo(MachineName, $"Start Preauth | Amount: " + minBalance);

            Iuc.SendSaleCommand(IucSerialPortInterfaceV2.Commands.Preauth(minBalance),
                (SaleApprove) =>
                {
                    result = true;
                    CurrentPreauthResponse = SaleApprove;
                    LogService.LogMagicPaymentInfo("Preauth Approved");
                    //End();
                    //callback?.Invoke(TransactionStatus.Success, SaleApprove, CardPaymentType.CREDITCARD, string.Empty);
                    SlackService.SendInfo(MachineName, "Preauth Approved");

                    isCmdFinished = true;
                },
                (SaleError) =>
                {
                    result = false;
                    var response = (SaleResponse)SaleError;
                    LogService.LogMagicPaymentInfo($"Preauth Error: [{response.ResponseCode}] {response.Message}");
                    CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.CantReadCardDialogMessage, 20);
                    SlackService.SendInfo(MachineName, $"Preauth Error: [{response.ResponseCode}] {response.Message}");
                    isCmdFinished = true;
                },
                (SaleCancel) =>
                {
                    LogService.LogMagicPaymentInfo("Preauth Cancelled");
                    SlackService.SendInfo(MachineName, "Preauth Cancelled");
                    isCmdFinished = true;
                    CustomerUINotificationService.SendDialogNotification("CANT DETECT YOUR CARD, PLEASE TRY TO START AGAIN AND INSERT YOUR CARD!");
                },
                (Callback) =>
                {
                    SlackService.SendInfo(MachineName, $"Preauth Message: {Callback}");
                    LogService.LogMagicPaymentInfo("Preauth Callback: " + Callback);
                }, 
                (OnCommandError) =>
                {
                    SlackService.SendInfo(MachineName, $"Preauth Error: {OnCommandError.ToString()}");

                    LogService.LogMagicPaymentInfo("PREAUTH COMMAND ERROR: " + OnCommandError.ToString());
                    isCmdFinished = true;
                });

            var timeout = 0;
            // Wait for cmd finish
            while (!isCmdFinished)
            {
                LogService.LogMagicPaymentInfo("Validating...");
                Thread.Sleep(1000);
                if (++timeout >= 90)
                {
                    LogService.LogMagicPaymentInfo("Validating Timeout");
                    SlackService.SendInfo(MachineName, "Preauth Timed out, no response from terminal!!!");

                    result = false;
                    break;
                }
            }

            isValidating = false;

            //Iuc.CleanCallBack();

            var message = $"Validate result: {result}";
            LogService.LogMagicPaymentInfo(message);

            return result;
        }

        public void Charge(int amount, Action<TransactionStatus, object, CardPaymentType, string> callback = null)
        {
            CustomerUINotificationService.DismissDialog();
            IsChargeFinished = false;
            LogService.LogMagicPaymentInfo($"Charge: {amount} | Payment mode:");
            SlackService.SendInfo(MachineName, $"Start Charging | Amount: {amount}");

            Thread.Sleep(200);
            Iuc.SendSaleCommand(IucSerialPortInterface.Commands.Sale(amount),
                (SaleApprove) =>
                {
                    LogService.LogMagicPaymentInfo("Charge Approve");
                    callback?.Invoke(TransactionStatus.Success, SaleApprove, CardPaymentType.CREDITCARD, string.Empty);
                    SlackService.SendInfo(MachineName, "Charge Approved");
                    IsChargeFinished = true;
                    //Iuc.CleanCallBack();

                },
                (SaleError) =>
                {
                    var response = (SaleResponse)SaleError;
                    LogService.LogMagicPaymentInfo($"Charge Error: [{response.ResponseCode}] {response.Message}");

                    //if (iucErrorCodes.Any(x => x.Equals(response.ResponseCode)))
                    //{
                    //    if (++errorRetryChargeTime <= 1)
                    //    {
                    //        LogService.LogInfo($"Retring to charge...");
                    //        SlackService.SendAlert(RfidFridgeSetting.Machine.Name, $"Found error code {response.ResponseCode}, retring to charge...");
                    //        Charge(amount, callback);
                    //    }
                    //}

                    callback?.Invoke(TransactionStatus.Error, SaleError, CardPaymentType.CREDITCARD, string.Empty);
                    SlackService.SendInfo(MachineName, $"Charge Error: [{response.ResponseCode}] {response.Message}");
                    IsChargeFinished = true;
                    //Iuc.CleanCallBack();

                },
                (SaleCancel) =>
                {
                    LogService.LogMagicPaymentInfo("Charge CC Cancel");
                    callback?.Invoke(TransactionStatus.Error, SaleCancel, CardPaymentType.CREDITCARD, string.Empty);
                    SlackService.SendInfo(MachineName, "Charge CC Cancel");
                    IsChargeFinished = true;
                    //Iuc.CleanCallBack();

                },
                (Callback) =>
                {
                    SlackService.SendInfo(MachineName, $"Charge Message: {Callback}");
                    LogService.LogMagicPaymentInfo("Charge Callback: " + Callback);
                    CustomerUINotificationService.SendDialogNotification(Callback);
                },
                (OnCommandError) =>
                {
                    callback?.Invoke(TransactionStatus.Error, null, CardPaymentType.CREDITCARD, string.Empty);
                    SlackService.SendInfo(MachineName, $"Charge Error: {OnCommandError.ToString()}");
                    LogService.LogMagicPaymentInfo("SALE COMMAND ERROR: " + OnCommandError.ToString());
                    IsChargeFinished = true;
                    //Iuc.CleanCallBack();

                });
        }

        public void Refund(Action<bool> callback = null)
        {
            CustomerUINotificationService.DismissDialog();


            var cardNumber = CurrentPreauthResponse.CardNumber;
            var expDate = CurrentPreauthResponse.ExpDate;
            var rrn = CurrentPreauthResponse.Rrn;
            var invoice = CurrentPreauthResponse.Invoice;

            LogService.LogMagicPaymentInfo("Cancelling Preauth");
            LogService.LogMagicPaymentInfo($"  - Card number: {cardNumber}");
            LogService.LogMagicPaymentInfo($"  - Exp Date: {expDate}");
            LogService.LogMagicPaymentInfo($"  - RRN: {rrn}");
            LogService.LogMagicPaymentInfo($"  - Invoice: {invoice}");
            LogService.LogMagicPaymentInfo($"  - TID: {Iuc.Tid} | Mid: {Iuc.Mid}");




            int.TryParse(RfidFridgeSetting.System.Payment.Magic.MinBalanceRequire, out int minBalance);

            SlackService.SendInfo(MachineName, $"Start Refund | Amount: " + minBalance);

            Iuc.SendSaleCommand(IucSerialPortInterfaceV2.Commands.CancelPreauth(cardNumber, expDate, rrn, invoice, Iuc.Tid, Iuc.Mid, minBalance),
            (SaleApprove) =>
            {
                LogService.LogMagicPaymentInfo("Cancel Preauth Approve");
                SlackService.SendInfo(MachineName, "Cancel Preauth Approve");
                callback?.Invoke(true);
                //Iuc.CleanCallBack();

            },
            (SaleError) =>
            {
                var response = (SaleResponse)SaleError;
                LogService.LogMagicPaymentInfo($"Cancel Preauth Error: [{response.ResponseCode}] {response.Message}");
                SlackService.SendInfo(MachineName, $"Cancel Preauth Error: [{response.ResponseCode}] {response.Message}");
                callback?.Invoke(false);
                //Iuc.CleanCallBack();

            },
            (SaleCancel) =>
            {
                LogService.LogMagicPaymentInfo("Cancel Preauth Cancel");
                SlackService.SendInfo(MachineName, "Cancel Preauth Cancel");
                callback?.Invoke(false);
                //Iuc.CleanCallBack();

            },
            (Callback) =>
            {
                SlackService.SendInfo(MachineName, $"Cancel Preauth Message: {Callback}");
                LogService.LogMagicPaymentInfo("Cancel Preauth Callback: " + Callback);
            },
            (OnCommandError) =>
            {
                SlackService.SendInfo(MachineName, $"Cancel Preauth Error: {OnCommandError.ToString()}");
                LogService.LogMagicPaymentInfo("PREAUTH COMMAND ERROR: " + OnCommandError.ToString());
                callback?.Invoke(false);
                //Iuc.CleanCallBack();

            });
        }

        

        public void End(bool isChargeSuccess = true)
        {
          
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

        }

        public DateTime GetLastPolling()
        {
            return DateTime.Now;
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


    }
}
