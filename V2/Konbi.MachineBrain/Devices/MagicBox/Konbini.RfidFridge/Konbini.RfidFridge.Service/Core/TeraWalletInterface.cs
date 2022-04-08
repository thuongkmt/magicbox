using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.DTO.Tera;
using Konbini.RfidFridge.Service.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Core
{
    public class TeraWalletInterface
    {
        public string BASE_URL { get; set; }
        public string TOKEN { get; set; }
        public int USER_ID { get; set; }

        public double RESERVE_AMOUNT { get; set; }

        private LogService LogService;
        private SlackService SlackService;
        private CustomerUINotificationService CustomerUINotificationService;

        public TeraWalletInterface(LogService logService, SlackService slackService, CustomerUINotificationService customerUINotificationService)
        {
            LogService = logService;
            SlackService = slackService;
            CustomerUINotificationService = customerUINotificationService;
        }

        public void Init()
        {
            BASE_URL = RfidFridgeSetting.System.Payment.TeraWallet.Host;
            RESERVE_AMOUNT = int.Parse(RfidFridgeSetting.System.Payment.TeraWallet.ReserveAmount);
        }

        public bool ValidateQr(string qrcode)
        {
            CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.TeraQRValidating, 99);
            LogService.LogWallet("Validating QR: " + qrcode);

            if (qrcode.Contains(";"))
            {
                var qr = qrcode.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                USER_ID = int.Parse(qr[0]);
                TOKEN = qrcode;//qr[1];
                var userBalance = GetBalance(USER_ID) * 100;

                LogService.LogWallet($"Validating QR Result | User ID: {USER_ID} | Token: {TOKEN} | Balance: {userBalance}");

                if (userBalance < 0)
                {
                    CustomerUINotificationService.SendDialogNotification("Failed to check balance - Server Error", 5);
                    return false;
                }

                if (userBalance >= RESERVE_AMOUNT)
                {
                    CustomerUINotificationService.DismissDialog();
                    return true;
                }
                else
                {
                    CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.TeraBalanceNotEnough, 5);
                    return false;
                }
            }
            else
            {
                CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.TeraQRValidateFail, 5);
                return false;
            }
        }

        public double GetBalance(int userId)
        {

            var url = new Uri(BASE_URL).AbsoluteUri;
            using (var httpClient = new HttpClient())
            {
                var requestUrl = $"{url}/wp-json/wp/v2/current_balance/{userId}?access_token={TOKEN}";
                LogService.LogWallet("GetBalance | requestUrl: " + requestUrl);

                using (var request = new HttpRequestMessage(new HttpMethod("GET"), requestUrl))
                {
                    //request.Headers.TryAddWithoutValidation("Cookie", "__cfduid=df5d09ad933e00fdbe36e9248505dc40c1591180112");

                    var response = httpClient.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var r = response.Content.ReadAsAsync<string>().Result;
                        double.TryParse(r, out double result);
                        return result;
                    }
                    else
                    {
                        LogService.LogWallet(response.ToString());
                        return -1;
                    }
                }
            }
        }

        public bool Charge(decimal amount, ref TereWalletMbChargeResponse response, List<InventoryDto> inventories = null)
        {
            response = new TereWalletMbChargeResponse
            {
                UserId = USER_ID.ToString()
            };
            amount /= 100m;
            LogService.LogWallet($"Charging user: {USER_ID} | Amount: {amount} | Token: {TOKEN}");
            var url = new Uri(BASE_URL).AbsoluteUri;
            using (var httpClient = new HttpClient())
            {
                var details = string.Empty;
                if (inventories != null)
                {
                    var listDetail = inventories.Select(x => $"{x.Product.ProductName}:{x.TagId}").ToList();
                    details = string.Join(" | ", listDetail);
                }

                var requestUrl = $"{url}/wp-json/wp/v2/wallet/{USER_ID}?access_token={TOKEN}&amount={amount}&type=debit&details={details}";
                LogService.LogWallet("GetBalance | requestUrl: " + requestUrl);

                using (var request = new HttpRequestMessage(new HttpMethod("POST"), requestUrl))
                {

                    var responseResult = httpClient.SendAsync(request).Result;
                    var r = responseResult.Content.ReadAsStringAsync().Result;

                    if (responseResult.IsSuccessStatusCode)
                    {
                        response.Message = "Payment Success";
                        return true;
                    }
                    else
                    {
                        LogService.LogWallet(responseResult.ToString());

                        response.Message = "Payment Failed";
                        return false;
                    }
                }
            }
        }
    }
}
