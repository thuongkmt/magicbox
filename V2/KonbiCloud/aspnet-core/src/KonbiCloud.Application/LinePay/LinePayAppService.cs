using Abp.Authorization;
using Abp.Configuration;
using KonbiCloud.Configuration;
using KonbiCloud.LinePay.Dtos;
using KonbiCloud.LinePay.Models;
using Konbini.Messages;
using Konbini.Messages.Enums;
using Konbini.Messages.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.LinePay
{
    [AbpAllowAnonymous]
    public class LinePayAppService : KonbiCloudAppServiceBase, ILinePayAppService
    {
        private LinePayClient _client;
        private readonly ISendMessageToMachineClientService _sendMessageToMachineService;
        public static Dictionary<string, Int64> _cache = new Dictionary<string, long>();

        public LinePayAppService(ISendMessageToMachineClientService sendMessageToMachineService)
        {
            _sendMessageToMachineService = sendMessageToMachineService;

            _client = new LinePayClient(
                "1605930549",                       // あなたの LINE PAY CHANNEL ID.
                "706035150ed6e54a64b3bc8392765112", // あなたの LINE PAY CHANNEL SECRET.
                true);                              // あなたの LINE PAY IS SANDBOX.
        }

        /// <summary>
        /// Payment Confirm API
        /// This API is used for a Merchant to complete its payment. The Merchant must call Confirm Payment API to
        /// actually complete the payment.However, when "capture" parameter is "false" on payment reservation, the
        /// payment status becomes AUTHORIZATION, and the payment is completed only after "Capture API" is called.
        /// </summary>
        /// <param name="machineId">Scanning client machine.</param>
        /// <param name="transactionId">Server Line transaction ID returned.</param>
        /// <returns></returns>
        public void GetConfirmLinePay(Guid machineId, Int64 transactionId, string regkey)
        {
            try
            {
                Logger.Info($"Start send message to MB.");
                _sendMessageToMachineService.SendQueuedMsgToMachines(new KeyValueMessage
                {
                    Key = MessageKeys.LinePayOpenLock,
                    MachineId = machineId,
                    Value = new LinePayDto
                    {
                        regkey = regkey,
                        transactionId = transactionId
                    }
                }, CloudToMachineType.ToMachineId);
                Logger.Info($"Finish send message to MB.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Expire regkey by LinePay {ex.Message}", ex);
                throw ex;
            }
        }

        public async Task GetFinishLinePay(Guid machineId, string regkey, Int64 transactionId, int amount, string productName)
        {
            try
            {
                Logger.Info($"Start Call API PreApprovedPay.");
                // Call API PreApprovedPay.
                await PreApprovedPay(regkey, amount, productName);

                Logger.Info($"Start Call API Refund.");
                // Call API Refund.
                await Refund(transactionId, 250);

                Logger.Info($"Call API Expire.");
                // Call API Expire.
                await Expire(regkey);

                Logger.Info($"send message to MB.");
                _sendMessageToMachineService.SendQueuedMsgToMachinesLinePay(new KeyValueMessage
                {
                    Key = MessageKeys.LinePayCloseLock,
                    MachineId = machineId,
                    Value = new LinePayFinishDto
                    {
                        regkey = regkey,
                        transactionId = transactionId,
                        amount = amount,
                        productName = productName
                    }
                }, CloudToMachineType.ToMachineId);
                Logger.Info($"Finish send message to MB.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Expire regkey by LinePay {ex.Message}", ex);
                throw ex;
            }
        }

        /// <summary>
        /// Preapproved Payment API
        /// When the payment type of the Reserve Payment API was set as PREAPPROVED, a regKey is returned with the
        /// payment result.Preapproved Payment API uses this regKey to directly complete a payment without using the LINE app.
        /// </summary>
        /// <param name="regkey">Registration Key</param>
        /// <returns></returns>
        public async Task<bool> PreApprovedPay(string regkey, int amount, string productName)
        {
            try
            {
                var reserve = new PreApprovedPay()
                {
                    Amount = amount,
                    Capture = true,
                    Currency = Currency.THB,
                    ProductName = productName,
                    OrderId = Guid.NewGuid().ToString()
                };
                var response = await _client.PreApprovedPayAsync(regkey, reserve);
                if (response.ReturnCode != "0000")
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"PreApprovedPay by LinePay {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Refund Payment API
        /// Requests refund of payments made with LINE Pay. To refund a payment, the LINE Pay user's payment transaction
        /// Id must be forwarded.A partial refund is also possible depending on the refund amount.
        /// </summary>
        /// <param name="transactionId">Transaction Id</param>
        /// <param name="amount">Refund Amount</param>
        /// <returns></returns>
        public async Task<bool> Refund(Int64 transactionId, int amount)
        {
            try
            {
                var response = await _client.RefundAsync(transactionId, amount);
                if (response.ReturnCode != "0000")
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Refund by LinePay {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// Expire regKey API
        /// Expires the regKey information registered for preapproved payment.Once the API is called, the regKey is no longer used for preapproved payments.
        /// </summary>
        /// <param name="regkey">Registration Key</param>
        /// <returns></returns>
        public async Task<bool> Expire(string regkey)
        {
            try
            {
                var response = await _client.RegKeyExpireAsync(regkey);
                if (response.ReturnCode != "0000")
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Expire regkey by LinePay {ex.Message}", ex);
                return false;
            }
        }
    }
}
