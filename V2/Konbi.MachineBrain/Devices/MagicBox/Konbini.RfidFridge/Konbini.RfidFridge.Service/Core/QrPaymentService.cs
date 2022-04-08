using Emgu.CV;
using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.DTO.GrabPay;
using Konbini.RfidFridge.Domain.DTO.Tera;
using Konbini.RfidFridge.Domain.Enums;
using Konbini.RfidFridge.Service.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Core
{
    public class QrPaymentService
    {
        #region Properties
        // public static string ChargingToken = null;
        #endregion

        #region Services
        public LogService LogService;
        public GrabpPayInterface GrabPayInterface;
        public SlackService SlackService;
        public TeraWalletInterface TeraWalletInterface;
        public CustomerUINotificationService CustomerUINotificationService;
        public CreditCardWalletInterface CreditCardWalletInterface;
        #endregion

        public QrPaymentType CurrentPaymentType { get; set; }
        public bool IsProcessing { get; set; }

        public Action<PaymentType> OnValidateQrSuccess { get; set; }


        public QrPaymentService(LogService logService, GrabpPayInterface grabpPayInterface, TeraWalletInterface teraWalletInterface, SlackService slackService,
            CustomerUINotificationService customerUINotificationService, CreditCardWalletInterface creditCardWalletInterface)
        {
            LogService = logService;
            GrabPayInterface = grabpPayInterface;
            TeraWalletInterface = teraWalletInterface;
            SlackService = slackService;
            CustomerUINotificationService = customerUINotificationService;
            CreditCardWalletInterface = creditCardWalletInterface;
        }

        public void Init(QrPaymentType type)
        {
            try
            {
                CurrentPaymentType = type;

                if (type != QrPaymentType.ALL)
                {
                    switch (CurrentPaymentType)
                    {
                        case QrPaymentType.GRABPAY:
                            GrabPayInterface.Init();
                            break;
                        case QrPaymentType.WALLET:
                            TeraWalletInterface.Init();
                            break;
                        case QrPaymentType.CREDITCARD_WALLET:
                            CreditCardWalletInterface.Init();
                            break;
                    }
                }
                else
                {
                    GrabPayInterface.Init();
                    CreditCardWalletInterface.Init();
                    TeraWalletInterface.Init();
                }

            }
            catch (Exception ex)
            {
                LogService.LogGrabPay(ex.ToString());
            }
        }

        public void Start()
        {

        }

        public void End()
        {

        }

        public bool Validate(string code)
        {
            try
            {
                if (IsProcessing)
                {
                    LogService.LogInfo("Processing Validating QR | Code: " + code);
                    return false;
                }
                IsProcessing = true;
                bool isValid = false;


                if (code.Contains(";"))
                {
                    CurrentPaymentType = QrPaymentType.WALLET;
                }
                else if (code.StartsWith("konbini"))
                {
                    CurrentPaymentType = QrPaymentType.CREDITCARD_WALLET;
                }
                else
                {
                    CurrentPaymentType = QrPaymentType.GRABPAY;
                }

                switch (CurrentPaymentType)
                {
                    case QrPaymentType.GRABPAY:
                        LogService.LogInfo("Validating QR | Code: " + code);
                        isValid = GrabPayInterface.ValidateQr(code);

                        if (isValid)
                        {
                            OnValidateQrSuccess?.Invoke(PaymentType.QR);
                            IsProcessing = false;
                        }
                        LogService.LogInfo($"Is QR Valid: {isValid}");
                        if (isValid)
                        {
                            LogService.LogInfo("OrigPartnerTxID: " + GrabPayInterface.OrigPartnerTxID);
                        }
                        IsProcessing = false;
                        break;
                    case QrPaymentType.WALLET:
                        LogService.LogInfo("Validating QR | Code: " + code);
                        isValid = TeraWalletInterface.ValidateQr(code);

                        if (isValid)
                        {
                            OnValidateQrSuccess?.Invoke(PaymentType.QR);
                            IsProcessing = false;
                        }
                        LogService.LogInfo($"Is QR Valid: {isValid}");
                        if (isValid)
                        {
                            LogService.LogInfo("User ID: " + TeraWalletInterface.USER_ID);
                        }
                        IsProcessing = false;
                        break;

                    case QrPaymentType.CREDITCARD_WALLET:
                        LogService.LogInfo("Validating QR | Code: " + code);
                        isValid = CreditCardWalletInterface.ValidateQr(code);
                        LogService.LogInfo($"Is QR Valid: {isValid}");

                        if (isValid)
                        {
                            OnValidateQrSuccess?.Invoke(PaymentType.QR);
                            IsProcessing = false;
                        }
                        //if (isValid)
                        //{
                        //    LogService.LogInfo("User ID: " + TeraWalletInterface.USER_ID);
                        //}
                        IsProcessing = false;
                        break;
                }
                CustomerUINotificationService.DismissDialog();
                return isValid;
            }
            catch (Exception ex)
            {
                LogService.LogError(ex.ToString());
                IsProcessing = false;
                return false;
            }
        }

        public void Charge(int amount, Action<TransactionStatus, object> callback = null, List<InventoryDto> inventories = null)
        {
            try
            {
                switch (CurrentPaymentType)
                {
                    case QrPaymentType.GRABPAY:
                        GrabPayMbChargeResponse response = new GrabPayMbChargeResponse();
                        LogService.LogInfo($"Charge QR | Amount: {amount} | OrigPartnerTxID: {GrabPayInterface.OrigPartnerTxID}");

                        var isSuccess = GrabPayInterface.Charge(amount, ref response);
                        LogService.LogInfo($"Charge QR Result: {isSuccess} | Message: {response.Message}");
                        callback?.Invoke(isSuccess == true ? TransactionStatus.Success : TransactionStatus.Error, response);
                        break;
                    case QrPaymentType.WALLET:
                        TereWalletMbChargeResponse teraResponse = new TereWalletMbChargeResponse();
                        LogService.LogInfo($"Charge QR | Amount: {amount} | UserID: {TeraWalletInterface.USER_ID}");

                        var isSuccess1 = TeraWalletInterface.Charge(amount, ref teraResponse, inventories);
                        LogService.LogInfo($"Charge QR Result: {isSuccess1} | Message: {teraResponse.Message}");
                        callback?.Invoke(isSuccess1 == true ? TransactionStatus.Success : TransactionStatus.Error, teraResponse);
                        break;

                    case QrPaymentType.CREDITCARD_WALLET:
                        CreditCardWalletResponse ccWalletResponse = new CreditCardWalletResponse();
                        LogService.LogInfo($"Charge QR | Amount: {amount} | UserID: {TeraWalletInterface.USER_ID}");

                        var isSuccess2 = CreditCardWalletInterface.Charge(amount, ref ccWalletResponse, inventories);

                        var txnStatus = isSuccess2 == true ? TransactionStatus.Success : TransactionStatus.Error;
                        if (amount == 0)
                        {
                            txnStatus = TransactionStatus.Cancelled;
                        }
                        LogService.LogInfo($"Charge QR Result: {isSuccess2} | IsSuccess: {ccWalletResponse.IsSuccess}");
                        callback?.Invoke(txnStatus, ccWalletResponse);
                        break;
                }


            }
            catch (Exception ex)
            {
                LogService.LogError(ex.ToString());
            }
        }


    }
}
