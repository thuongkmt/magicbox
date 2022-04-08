using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.DTO.Tera;
using Konbini.RfidFridge.Service.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Core
{
    public class CreditCardWalletInterface
    {
        public string BASE_URL { get; set; }
        public string TOKEN { get; set; }
        public int USER_ID { get; set; }

        public double RESERVE_AMOUNT { get; set; }

        private LogService LogService;
        private SlackService SlackService;
        private CustomerUINotificationService CustomerUINotificationService;
        public CreditCardWalletInterface(LogService logService, SlackService slackService, CustomerUINotificationService customerUINotificationService)
        {
            LogService = logService;
            SlackService = slackService;
            CustomerUINotificationService = customerUINotificationService;
        }

        public void Init()
        {
            BASE_URL = RfidFridgeSetting.System.Payment.CreditCardWallet.Host;
            //RESERVE_AMOUNT = int.Parse(RfidFridgeSetting.System.Payment.CreditCardWallet.ReserveAmount);
        }

        public bool ValidateQr(string qrcode)
        {
            CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.QRValidating, 99);
            LogService.LogWallet("Validating QR: " + qrcode);

            TOKEN = qrcode;//qr[1];
            var isOK = Validate(TOKEN);

            LogService.LogWallet($"Validating QR Result | Token: {TOKEN} | Result: {isOK}");

            if (isOK)
            {
                CustomerUINotificationService.DismissDialog();
                return true;
            }
            else
            {
                CustomerUINotificationService.SendDialogNotification(RfidFridgeSetting.CustomerUI.Messages.QRUnsuccess, 15);
                return false;
            }

        }

        public bool Validate(string qr)
        {
            try
            {
                var response = new CreditCardWalletValidateQrResponse();

                LogService.LogWallet($"Validateing QR: {qr}");
                var url = new Uri(BASE_URL).AbsoluteUri;
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(url);
                    var request = new HttpRequestMessage(HttpMethod.Post, "/wp-json/wc/v3/magic-box/omise/customer/token");

                    var keyValues = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("access_token", TOKEN)
                };

                    request.Content = new FormUrlEncodedContent(keyValues);
                    var responseResult = httpClient.SendAsync(request).Result;
                    LogService.LogWallet(responseResult.ToString());

                    var r = responseResult.Content.ReadAsStringAsync().Result;
                    LogService.LogWallet(r);

                    response = JsonConvert.DeserializeObject<CreditCardWalletValidateQrResponse>(r);

                    return responseResult.IsSuccessStatusCode ? !response.Expired : false;
                }
            }
            catch (Exception ex)
            {
                LogService.LogWallet(ex.ToString());
                return false;
            }
        }


        public bool Charge(decimal amount, ref CreditCardWalletResponse response, List<InventoryDto> inventories = null)
        {
            response = new CreditCardWalletResponse();

            try
            {
                if (amount > 0)
                {

                    //amount /= 100m;
                    LogService.LogWallet($"Charging user: {USER_ID} | Amount: {amount} | Token: {TOKEN}");
                    var url = new Uri(BASE_URL).AbsoluteUri;
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.BaseAddress = new Uri(url);
                        var request = new HttpRequestMessage(HttpMethod.Post, "/wp-json/wc/v3/magic-box/omise/customer/charge");

                        var keyValues = new List<KeyValuePair<string, string>>
                            {
                                new KeyValuePair<string, string>("access_token", TOKEN),
                                new KeyValuePair<string, string>("amount", amount.ToString()),
                                new KeyValuePair<string, string>("currency", "sgd")
                            };

                        request.Content = new FormUrlEncodedContent(keyValues);
                        var responseResult = httpClient.SendAsync(request).Result;
                        LogService.LogWallet(responseResult.ToString());

                        if (responseResult.IsSuccessStatusCode)
                        {
                            var r = responseResult.Content.ReadAsStringAsync().Result;
                            response = JsonConvert.DeserializeObject<CreditCardWalletResponse>(r);
                            LogService.LogWallet(r);

                            return response.IsSuccess;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }
                else
                {
                    response.IsSuccess = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogService.LogWallet(ex.ToString());
                response.IsSuccess = false;
                return false;
            }
        }
    }
}
