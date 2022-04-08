using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.DTO.GrabPay;
using Konbini.RfidFridge.Domain.Enums;
using Konbini.RfidFridge.Service.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Core
{
    public class GrabpPayInterface
    {
        private string HOST = "https://partner-gateway.grab.com";
        private static string[] HTTP_METHOD = { "POST", "GET", "PUT" };
        private static string[] CONTENT_TYPE = { "application/json", "application/x-www-form-urlencoded" };
        private static string CURRENCY_CODE = ListOfSupportedCurrency.SGD.ToString();

        private string PARTNER_ID = "c589284f-39be-4351-b29f-d44b02ddb56d";
        private string PARTNER_SECRET = "U0OaTpmGJtV0EYkI";

        private string GRAB_ID = "6cead152-561f-4373-b3cb-5dc23538215f";
        private string TERMINAL_ID = "2af6400b6485cc619cc11cadc";
        private int RESERVED_AMOUNT = 2000;

        #region Services
        public LogService LogService;
        public GrabPayService utility;
        private CustomerUINotificationService CustomerUINotificationService;
        private SlackService SlackService;
        #endregion

        public string OrigPartnerTxID { get; set; }
        private GrabPayMbChargeResponse CurrentTransactionChargeResponse { get; set; }
        public GrabpPayInterface(LogService logService,
            GrabPayService grabPayService,
            CustomerUINotificationService customerUINotificationService,
            SlackService slackService)
        {
            LogService = logService;
            utility = grabPayService;
            SlackService = slackService;
            CustomerUINotificationService = customerUINotificationService;
            CurrentTransactionChargeResponse = new GrabPayMbChargeResponse();
        }

        /// <summary>
        /// Init params from setting
        /// </summary>
        public void Init()
        {
            HOST = RfidFridgeSetting.System.Payment.GrabPay.Host;
            PARTNER_ID = RfidFridgeSetting.System.Payment.GrabPay.PartnerId;
            PARTNER_SECRET = RfidFridgeSetting.System.Payment.GrabPay.PartnerSecret;
            GRAB_ID = RfidFridgeSetting.System.Payment.GrabPay.GrabId;
            TERMINAL_ID = RfidFridgeSetting.System.Payment.GrabPay.TerminalId;

            RESERVED_AMOUNT = int.Parse(RfidFridgeSetting.System.Payment.GrabPay.ReserveAmount);

            LogService.LogGrabPay($"HOST:{HOST}");
            LogService.LogGrabPay($"PARTNER_ID:{PARTNER_ID}");
            LogService.LogGrabPay($"PARTNER_SECRET:{PARTNER_SECRET}");
            LogService.LogGrabPay($"GRAB_ID:{GRAB_ID}");
            LogService.LogGrabPay($"TERMINAL_ID:{TERMINAL_ID}");
            LogService.LogGrabPay($"RESERVED_AMOUNT:{RESERVED_AMOUNT}");

        }

        /// <summary>
        /// 4.1 Create Order (Merchant Presented QRCode) 
        /// Creates a payment order with a unique reference (txID) and returns a QRCodde string encoded with merchant detail, amount and txID. 
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseCreateOrder> CreateOrderMerchantPresentedQRCode(int amount)
        {
            RequestCreateOrder requestCreateOrder = new RequestCreateOrder();
            requestCreateOrder.amount = amount;
            requestCreateOrder.msgID = utility.GenerateMsgID();
            requestCreateOrder.grabID = GRAB_ID;
            requestCreateOrder.terminalID = TERMINAL_ID;
            requestCreateOrder.currency = CURRENCY_CODE;
            requestCreateOrder.partnerTxID = utility.GeneratePartnerTxID();

            string requestURL = HOST + "/grabpay/partner/v1/terminal/qrcode/create";
            string requestBody = "{\n\t\"amount\":" + requestCreateOrder.amount +
                ",\n\t\"msgID\":\"" + requestCreateOrder.msgID +
                "\",\n\t\"grabID\":\"" + GRAB_ID +
                "\",\n\t\"terminalID\":\"" + TERMINAL_ID +
                "\",\n\t\"currency\":\"" + CURRENCY_CODE +
                "\",\n\t\"partnerTxID\":\"" + requestCreateOrder.partnerTxID +
                "\"\n}";
            string timestamp = DateTime.UtcNow.ToString("ddd, dd MMM yyy HH:mm:ss 'GMT'");
            string authorization = utility.GenerateHMACSignature(PARTNER_ID, PARTNER_SECRET, HTTP_METHOD[0], requestURL, CONTENT_TYPE[0], requestBody, timestamp);

            CreateLogRequestPresentedQRCode(requestCreateOrder, timestamp, authorization);

            ResponseCreateOrder result = new ResponseCreateOrder();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod(HTTP_METHOD[0]), requestURL))
                    {
                        request.Headers.TryAddWithoutValidation("Authorization", authorization);
                        request.Headers.TryAddWithoutValidation("Date", timestamp);
                        request.Headers.TryAddWithoutValidation("Accept", "*");

                        request.Content = new StringContent(requestBody);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(CONTENT_TYPE[0]);

                        var response = await httpClient.SendAsync(request);
                        var content = "";
                        using (var sr = new StreamReader(await response.Content.ReadAsStreamAsync()))
                        {
                            content = sr.ReadToEnd();
                        }
                        result = JsonConvert.DeserializeObject<ResponseCreateOrder>(content);
                        CreateLogResponsePresentedQRCode(result);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.LogGrabPay(ex.ToString());
                return result;
            }
        }

        /// <summary>
        /// 4.2 Inquiry 
        /// Returns details for a payment transaction or refund transaction.
        /// </summary>
        /// <param name="partnerTxID"></param>
        /// <returns></returns>
        public async Task<ResponseInquiry> Inquiry(string partnerTxID)
        {
            RequestInquiry requestInquiry = new RequestInquiry();
            requestInquiry.msgID = utility.GenerateMsgID();
            requestInquiry.grabID = GRAB_ID;
            requestInquiry.terminalID = TERMINAL_ID;
            requestInquiry.currency = CURRENCY_CODE;
            requestInquiry.txType = ListOfTxType.P2M.ToString();
            requestInquiry.partnerTxID = utility.GeneratePartnerTxID();

            string requestURL = HOST + $"/grabpay/partner/v1/terminal/transaction/{partnerTxID}?"
                + $"msgID={requestInquiry.msgID}&"
                + $"grabID={requestInquiry.grabID}&"
                + $"terminalID={requestInquiry.terminalID}&"
                + $"currency={requestInquiry.currency}&"
                + $"txType={requestInquiry.txType}";
            string requestBody = "";
            string timestamp = DateTime.UtcNow.ToString("ddd, dd MMM yyy HH:mm:ss 'GMT'");
            string authorization = utility.GenerateHMACSignature(PARTNER_ID, PARTNER_SECRET, HTTP_METHOD[1], requestURL, CONTENT_TYPE[1], requestBody, timestamp);

            //CreateLogRequestInquiry(requestInquiry, timestamp, authorization);

            ResponseInquiry result = new ResponseInquiry();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod(HTTP_METHOD[1]), requestURL))
                    {
                        request.Headers.TryAddWithoutValidation("Authorization", authorization);
                        request.Headers.TryAddWithoutValidation("Date", timestamp);
                        request.Headers.TryAddWithoutValidation("Accept", "*");

                       // request.Content = new StringContent(requestBody);
                        //request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(CONTENT_TYPE[1]);

                        var response = await httpClient.SendAsync(request);
                        var content = "";
                        using (var sr = new StreamReader(await response.Content.ReadAsStreamAsync()))
                        {
                            content = sr.ReadToEnd();
                        }
                        result = JsonConvert.DeserializeObject<ResponseInquiry>(content);
                        CreateLogResponseInquiry(result);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
                LogService?.LogGrabPay(ex.ToString());
                return result;
            }
        }

        /// <summary>
        /// 4.3 Cancel
        /// Cancels a pending payment. Cacnel can be done when the payment status is still unknown after 30 seconds.
        /// Payments which have been successfully charged, cannot be cancelled, use the /refund method instead. 
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseCancel> Cancel(string origPartnerTxID)
        {
            RequestCancel requestCancel = new RequestCancel();
            requestCancel.msgID = utility.GenerateMsgID();
            requestCancel.grabID = GRAB_ID;
            requestCancel.terminalID = TERMINAL_ID;
            requestCancel.currency = CURRENCY_CODE;
            requestCancel.origPartnerTxID = origPartnerTxID;
            requestCancel.partnerTxID = utility.GeneratePartnerTxID();

            string requestURL = HOST + $"/grabpay/partner/v1/terminal/transaction/{origPartnerTxID}/cancel";
            string requestBody = "{\n\t\"msgID\":\"" + requestCancel.msgID +
                "\",\n\t\"grabID\":\"" + requestCancel.grabID +
                "\",\n\t\"terminalID\":\"" + requestCancel.terminalID +
                "\",\n\t\"currency\":\"" + requestCancel.currency +
                "\",\n\t\"origTxID\":\"" + requestCancel.origPartnerTxID +
                "\",\n\t\"partnerTxID\":\"" + requestCancel.partnerTxID +
                "\"\n}";
            string timestamp = DateTime.UtcNow.ToString("ddd, dd MMM yyy HH:mm:ss 'GMT'");
            string authorization = utility.GenerateHMACSignature(PARTNER_ID, PARTNER_SECRET, HTTP_METHOD[2], requestURL, CONTENT_TYPE[0], requestBody, timestamp);

            CreateLogRequestCancel(requestCancel, timestamp, authorization);

            ResponseCancel result = new ResponseCancel();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod(HTTP_METHOD[2]), requestURL))
                    {
                        request.Headers.TryAddWithoutValidation("Authorization", authorization);
                        request.Headers.TryAddWithoutValidation("Date", timestamp);
                        request.Headers.TryAddWithoutValidation("Accept", "*");

                        request.Content = new StringContent(requestBody);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(CONTENT_TYPE[0]);

                        var response = await httpClient.SendAsync(request);
                        result.code = (int)response.StatusCode;
                        CreateLogResponseCancel(result);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.LogGrabPay(ex.ToString());
                return result;
            }
        }

        /// <summary>
        /// 4.4 Refund 
        /// Refunds a previously successful payment, returning a unique refund reference(txID) for this request. 
        /// Refunding can be done on the full amount for any transactions, or a partial amount if the transaction is paid by GrabPay in full price. 
        /// Multiple (partial) refunds will be accepted as long as their sum doesn't exceed the charged amount. 
        /// You can only request refund within 30 days of the charge. 
        /// </summary>
        /// <param name="origPartnerTxID"></param>
        /// <returns></returns>
        public async Task<ResponseRefund> Refund(string origPartnerTxID, Int64 amount, string reason = "")
        {
            RequestRefund requestRefund = new RequestRefund();
            requestRefund.msgID = utility.GenerateMsgID();
            requestRefund.grabID = GRAB_ID;
            requestRefund.terminalID = TERMINAL_ID;
            requestRefund.currency = CURRENCY_CODE;
            requestRefund.amount = amount;
            requestRefund.reason = reason;
            requestRefund.origPartnerTxID = origPartnerTxID;
            requestRefund.partnerTxID = utility.GeneratePartnerTxID();

            string requestURL = HOST + $"/grabpay/partner/v1/terminal/transaction/{origPartnerTxID}/refund";
            string requestBody = "{\n\t\"msgID\":\"" + requestRefund.msgID +
                "\",\n\t\"grabID\":\"" + requestRefund.grabID +
                "\",\n\t\"terminalID\":\"" + requestRefund.terminalID +
                "\",\n\t\"currency\":\"" + requestRefund.currency +
                "\",\n\t\"amount\":" + requestRefund.amount +
                ",\n\t\"reason\":\"" + requestRefund.reason +
                "\",\n\t\"partnerTxID\":\"" + requestRefund.partnerTxID +
                "\"\n}";
            string timestamp = DateTime.UtcNow.ToString("ddd, dd MMM yyy HH:mm:ss 'GMT'");
            string authorization = utility.GenerateHMACSignature(PARTNER_ID, PARTNER_SECRET, HTTP_METHOD[2], requestURL, CONTENT_TYPE[0], requestBody, timestamp);

            CreateLogRequestRefund(requestRefund, timestamp, authorization);
            LogService.LogGrabPay("HOST: " + HOST);
            LogService.LogGrabPay("requestBody: " + requestBody);
            LogService.LogGrabPay("timestamp: " + timestamp);
            LogService.LogGrabPay("authorization: " + authorization);

            ResponseRefund result = new ResponseRefund();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod(HTTP_METHOD[2]), requestURL))
                    {
                        request.Headers.TryAddWithoutValidation("Authorization", authorization);
                        request.Headers.TryAddWithoutValidation("Date", timestamp);
                        request.Headers.TryAddWithoutValidation("Accept", "*");

                        request.Content = new StringContent(requestBody);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(CONTENT_TYPE[0]);

                        var response = await httpClient.SendAsync(request);
                        var content = "";
                        using (var sr = new StreamReader(await response.Content.ReadAsStreamAsync()))
                        {
                            content = sr.ReadToEnd();
                        }
                        result = JsonConvert.DeserializeObject<ResponseRefund>(content);
                        CreateLogResponseRefund(result);

                        LogService.LogGrabPay("HTTP Response: " + response);

                        LogService.LogGrabPay("Response: " + content);

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.LogGrabPay(ex.ToString());
                return result;
            }
        }

        public bool ValidateQr(string qrcode)
        {
            CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.QRValidating, 99);
            // Validate by try to reserve amount
            var result = PerformTransactionConsumerPresentedQR(qrcode, RESERVED_AMOUNT).Result;
            CurrentTransactionChargeResponse.TransactionId = result.txID;

            // CreateLogResponsePerformTransactionConsumerPresentedQR(result);

            if (result.status == ListOfStatus.success.ToString())
            {
                CustomerUINotificationService.DismissDialog();
                return true;
            }
            else
            {
                SlackService.SendInfo(RfidFridgeSetting.Machine.Name, "[GRABPAY] Invalid QR: " + result.errMsg);
                CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.QRUnsuccess + $"<br> {result.errMsg}");
                return false;
            }
            //return result.status == ValidateResponseGrab(result.status, result.code, result.reason);
        }


        public bool Charge(int amount, ref GrabPayMbChargeResponse reponse)
        {
            var refundAmount = RESERVED_AMOUNT - amount;

            if(refundAmount <= 0)
            {
                // This case no need to refund
                reponse = CurrentTransactionChargeResponse;
                return true;
            }

            var result = Refund(OrigPartnerTxID, refundAmount, string.Empty).Result;
            CurrentTransactionChargeResponse.RefundId = result.txID;
            CurrentTransactionChargeResponse.Message = result.reason;
            reponse = CurrentTransactionChargeResponse;

            return result.status == ListOfStatus.success.ToString();// == ValidateResponseGrab(result.status, result.code, result.reason);
        }

        /// <summary>
        /// Validate Response of Grab and send Notification.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="code"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public string ValidateResponseGrab(string status, string code, string reason)
        {
            if (status != null)
            {
                if (status == ListOfStatus.success.ToString())
                {
                    CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.QRStatusSuccess);
                    return ListOfStatus.success.ToString();
                }
                else if (status == ListOfStatus.failed.ToString())
                {
                    CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.QRStatusFailed);
                    return ListOfStatus.failed.ToString();
                }
                else if (status == ListOfStatus.unknown.ToString())
                {
                    CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.QRStatusUnknown);
                    return ListOfStatus.unknown.ToString();
                }
                else if (status == ListOfStatus.pending.ToString())
                {
                    CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.QRStatusPending);
                    return ListOfStatus.pending.ToString();
                }
                else if (status == ListOfStatus.bad_debt.ToString())
                {
                    CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.QRStatusBad_debt);
                    return ListOfStatus.bad_debt.ToString();
                }
            }
            else
            {
                switch (code)
                {
                    case "5001":
                        CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.GrabError5001);
                        break;
                    case "5003":
                        CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.GrabError5003);
                        break;
                    case "4040":
                        CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.GrabError4040);
                        break;
                    case "4041":
                        CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.GrabError4041);
                        break;
                    case "4097":
                        CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.GrabError4097);
                        break;
                    case "40011":
                        CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.GrabError40011);
                        break;
                    case "40012":
                        CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.GrabError40012);
                        break;
                    case "10912":
                        CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.GrabError40912);
                        break;
                    case "40913":
                        CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.GrabError40913);
                        break;
                    default:
                        CustomerUINotificationService.SendDialogNotification(reason);
                        break;
                }
            }
            return ListOfStatus.failed.ToString();
        }

        /// <summary>
        /// 4.5 Perform Transaction (Consumer Presented QR) 
        /// Performs a payment transaction which charges from the wallet associated with the request qrcode and payouts to request merchant grabID. 
        /// </summary>
        /// <returns></returns>
        public async Task<ResponsePerformTransaction> PerformTransactionConsumerPresentedQR(string code, Int64 amount)
        {
            RequestPerformTransaction requestPerformTransaction = new RequestPerformTransaction();
            requestPerformTransaction.amount = amount;
            requestPerformTransaction.msgID = utility.GenerateMsgID();
            requestPerformTransaction.grabID = GRAB_ID;
            requestPerformTransaction.terminalID = TERMINAL_ID;
            requestPerformTransaction.currency = CURRENCY_CODE;
            OrigPartnerTxID = requestPerformTransaction.partnerTxID = utility.GeneratePartnerTxID();
            requestPerformTransaction.code = code;

            string requestURL = HOST + "/grabpay/partner/v1/terminal/transaction/perform";
            string requestBody = "{\n\t\"amount\":" + requestPerformTransaction.amount +
                ",\n\t\"msgID\":\"" + requestPerformTransaction.msgID +
                "\",\n\t\"grabID\":\"" + GRAB_ID +
                "\",\n\t\"terminalID\":\"" + TERMINAL_ID +
                "\",\n\t\"currency\":\"" + CURRENCY_CODE +
                "\",\n\t\"partnerTxID\":\"" + requestPerformTransaction.partnerTxID +
                "\",\n\t\"code\":\"" + requestPerformTransaction.code +
                "\"\n}";
            string timestamp = DateTime.UtcNow.ToString("ddd, dd MMM yyy HH:mm:ss 'GMT'");
            string authorization = utility.GenerateHMACSignature(PARTNER_ID, PARTNER_SECRET, HTTP_METHOD[0], requestURL, CONTENT_TYPE[0], requestBody, timestamp);

            CreateLogRequestPerformTransactionConsumerPresentedQR(requestPerformTransaction, timestamp, authorization);

            LogService.LogGrabPay("requestURL: " + requestURL);
            LogService.LogGrabPay("requestBody: " + requestBody);
            LogService.LogGrabPay("authorization: " + authorization);
            LogService.LogGrabPay("timestamp: " + timestamp);

            ResponsePerformTransaction result = new ResponsePerformTransaction();
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod(HTTP_METHOD[0]), requestURL))
                    {
                        request.Headers.TryAddWithoutValidation("Authorization", authorization);
                        request.Headers.TryAddWithoutValidation("Date", timestamp);
                        request.Headers.TryAddWithoutValidation("Accept", "*");

                        request.Content = new StringContent(requestBody);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(CONTENT_TYPE[0]);

                        var response = await httpClient.SendAsync(request);
                        var content = "";
                        using (var sr = new StreamReader(await response.Content.ReadAsStreamAsync()))
                        {
                            content = sr.ReadToEnd();
                        }
                        result = JsonConvert.DeserializeObject<ResponsePerformTransaction>(content);
                        CreateLogResponsePerformTransactionConsumerPresentedQR(result);
                        LogService.LogGrabPay("HTTP Response: " + response);

                        LogService.LogGrabPay("Response: " + content);

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                LogService.LogGrabPay(ex.ToString());
                return result;
            }
        }

        #region CreateLog PresentedQRCode
        public void CreateLogRequestPresentedQRCode(RequestCreateOrder request, string timestamp, string authorization)
        {
            LogService.LogGrabPay("==============Request Create Order (Merchant Presented QRCode).=============");
            LogService.LogGrabPay("msgID:" + request.msgID);
            LogService.LogGrabPay("grabID:" + request.grabID);
            LogService.LogGrabPay("terminalID:" + request.terminalID);
            LogService.LogGrabPay("currency:" + request.currency);
            LogService.LogGrabPay("amount:" + request.amount);
            LogService.LogGrabPay("partnerTxID:" + request.partnerTxID);
            LogService.LogGrabPay("DateTime:" + timestamp);
            LogService.LogGrabPay("HMAC Signature:" + authorization);
        }

        public void CreateLogResponsePresentedQRCode(ResponseCreateOrder response)
        {
            LogService.LogGrabPay("");
            LogService.LogGrabPay("==============Response Create Order (Merchant Presented QRCode).=============");
            LogService.LogGrabPay("msgID:" + response.msgID);
            LogService.LogGrabPay("qrcode:" + response.qrcode);
            LogService.LogGrabPay("txID:" + response.txID);
            LogService.LogGrabPay("");
        }
        #endregion

        #region CreateLog Inquiry 
        public void CreateLogRequestInquiry(RequestInquiry request, string timestamp, string authorization)
        {
            LogService.LogGrabPay("==============Request Inquiry.=============");
            LogService.LogGrabPay("msgID:" + request.msgID);
            LogService.LogGrabPay("grabID:" + request.grabID);
            LogService.LogGrabPay("terminalID:" + request.terminalID);
            LogService.LogGrabPay("currency:" + request.currency);
            LogService.LogGrabPay("txType:" + request.txType);
            LogService.LogGrabPay("partnerTxID:" + request.partnerTxID);
            LogService.LogGrabPay("DateTime:" + timestamp);
            LogService.LogGrabPay("HMAC Signature:" + authorization);
        }

        public void CreateLogResponseInquiry(ResponseInquiry response)
        {
            LogService.LogGrabPay("");
            LogService.LogGrabPay("==============Response Inquiry.=============");
            LogService.LogGrabPay("msgID:" + response.msgID);
            LogService.LogGrabPay("txID:" + response.txID);
            LogService.LogGrabPay("status:" + response.status);
            LogService.LogGrabPay("currency:" + response.currency);
            LogService.LogGrabPay("amount:" + response.amount);
            LogService.LogGrabPay("updated:" + response.updated);
            LogService.LogGrabPay("errMsg:" + response.errMsg);
            LogService.LogGrabPay("additionalInfo:" + response.additionalInfo);
            LogService.LogGrabPay("");
        }
        #endregion

        #region CreateLog Cancel 
        public void CreateLogRequestCancel(RequestCancel request, string timestamp, string authorization)
        {
            LogService.LogGrabPay("==============Request Inquiry.=============");
            LogService.LogGrabPay("msgID:" + request.msgID);
            LogService.LogGrabPay("grabID:" + request.grabID);
            LogService.LogGrabPay("terminalID:" + request.terminalID);
            LogService.LogGrabPay("currency:" + request.currency);
            LogService.LogGrabPay("origPartnerTxID:" + request.origPartnerTxID);
            LogService.LogGrabPay("partnerTxID:" + request.partnerTxID);
            LogService.LogGrabPay("DateTime:" + timestamp);
            LogService.LogGrabPay("HMAC Signature:" + authorization);
        }

        public void CreateLogResponseCancel(ResponseCancel response)
        {
            LogService.LogGrabPay("");
            LogService.LogGrabPay("==============Response Inquiry.=============");
            LogService.LogGrabPay("status code:" + response.code);
            LogService.LogGrabPay("");
        }
        #endregion

        #region CreateLog Refund  
        public void CreateLogRequestRefund(RequestRefund request, string timestamp, string authorization)
        {
            LogService.LogGrabPay("==============Request Refund.=============");
            LogService.LogGrabPay("msgID:" + request.msgID);
            LogService.LogGrabPay("grabID:" + request.grabID);
            LogService.LogGrabPay("terminalID:" + request.terminalID);
            LogService.LogGrabPay("amount:" + request.amount);
            LogService.LogGrabPay("currency:" + request.currency);
            LogService.LogGrabPay("partnerTxID:" + request.partnerTxID);
            LogService.LogGrabPay("origPartnerTxID:" + request.origPartnerTxID);
            LogService.LogGrabPay("reason:" + request.reason);
            LogService.LogGrabPay("DateTime:" + timestamp);
            LogService.LogGrabPay("HMAC Signature:" + authorization);
        }

        public void CreateLogResponseRefund(ResponseRefund response)
        {
            LogService.LogGrabPay("");
            LogService.LogGrabPay("==============Response Refund.=============");
            LogService.LogGrabPay("msgID:" + response.msgID);
            LogService.LogGrabPay("txID:" + response.txID);
            LogService.LogGrabPay("status:" + response.status);
            LogService.LogGrabPay("");
        }
        #endregion

        #region CreateLog  Perform Transaction (Consumer Presented QR)  
        public void CreateLogRequestPerformTransactionConsumerPresentedQR(RequestPerformTransaction request, string timestamp, string authorization)
        {

            LogService.LogGrabPay("==============Request Perform Transaction (Consumer Presented QR).=============");

            LogService.LogGrabPay("msgID:" + request.msgID);
            LogService.LogGrabPay("grabID:" + request.grabID);
            LogService.LogGrabPay("terminalID:" + request.terminalID);
            LogService.LogGrabPay("currency:" + request.currency);
            LogService.LogGrabPay("amount:" + request.amount);
            LogService.LogGrabPay("partnerTxID:" + request.partnerTxID);
            LogService.LogGrabPay("code:" + request.code);
            LogService.LogGrabPay("additionalInfo:" + string.Join(",", Convert.ToString(request.additionalInfo)));
            LogService.LogGrabPay("DateTime:" + timestamp);
            LogService.LogGrabPay("HMAC Signature:" + authorization);
        }

        public void CreateLogResponsePerformTransactionConsumerPresentedQR(ResponsePerformTransaction response)
        {
            LogService.LogGrabPay("");
            LogService.LogGrabPay("==============Response Perform Transaction (Consumer Presented QR).=============");
            LogService.LogGrabPay("CODE:" + response.code);
            LogService.LogGrabPay("REASON:" + response.reason);
            LogService.LogGrabPay("msgID:" + response.msgID);
            LogService.LogGrabPay("txID:" + response.txID);
            LogService.LogGrabPay("status:" + response.status);
            LogService.LogGrabPay("currency:" + response.currency);
            LogService.LogGrabPay("amount:" + response.amount);
            LogService.LogGrabPay("updated:" + response.updated);
            LogService.LogGrabPay("errMsg:" + response.errMsg);
            LogService.LogGrabPay("additionalInfo:" + response.additionalInfo);
            LogService.LogGrabPay("");
        }
        #endregion
    }
}
