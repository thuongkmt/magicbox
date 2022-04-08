using Konbini.RfidFridge.Common;
using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.Entities;
using Konbini.RfidFridge.Service.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace Konbini.RfidFridge.Service.Util
{

    public class CustomerCloudApiService
    {
        private string _baseUrl;
        private string _userName;
        private string _password;
        private string _token;
        private string _tenant;
        private int _minBalance;


        private LogService LogService;
        public CustomerCloudApiService(LogService logService)
        {
            LogService = logService;
        }

        public string GetToken()
        {
            return this._token;
        }

        public string GetUrl()
        {
            return this._baseUrl;
        }

        public void Init()
        {
            _baseUrl = RfidFridgeSetting.System.CustomerCloud.Url;
            _userName = RfidFridgeSetting.System.CustomerCloud.Username;
            _password = RfidFridgeSetting.System.CustomerCloud.Password;
            _tenant = RfidFridgeSetting.System.CustomerCloud.TenantId;
            int.TryParse(RfidFridgeSetting.System.CustomerCloud.QrMinbalanceRequire, out _minBalance);

        }
        private string GetStringAsync(string api)
        {
            var client = new HttpClient { BaseAddress = new Uri(_baseUrl) };
            dynamic token = GetToken();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);

            var response = client.GetAsync(api).Result;
            dynamic res;
            using (var content = response.Content)
            {
                var result = content.ReadAsStringAsync();
                res = result.Result;
            }
            return string.IsNullOrEmpty(res) ? null : res;
        }



        public bool ValidateQr(string code, ref string chargingToken)
        {
            var tryTime = 0;
            var result = false;

            try
            {
            Retry:
                tryTime++;
                var token = GetToken();
                chargingToken = null;
                this.LogService.LogMachineAdminApi("Validating QR | Code = " + code);

                var client = new HttpClient
                {
                    BaseAddress = new Uri(_baseUrl)
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var paymentDetail = string.Empty;
                _minBalance = 0;
                var authCode = new ValidateByAuthCodeDto
                {
                    Code = code,
                    BalanceGreaterThan = _minBalance,
                    IsCardLinked = true
                };

                var tDto = JsonConvert.SerializeObject(authCode);
                this.LogService.LogMachineAdminApi("Validating QR | DTO = " + tDto);

                var response = client.PostAsJsonAsync($"api/services/app/Customer/ValidateByAuthCode", authCode).Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsAsync<dynamic>().Result;
                    if (json.result != null)
                    {
                        var isValid = (bool)json.result.isValid;
                        result = isValid;
                        chargingToken = json.result.token;
                    }
                    LogService.LogMachineAdminApi($"Result: {json}");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (tryTime <= 3)
                    {
                        Thread.Sleep(1000);
                        RenewToken();
                        goto Retry;
                    }
                    else
                    {
                        this.LogService.LogMachineAdminApi("Failed to get Token!");
                    }
                }
                else
                {
                    LogService.LogMachineAdminApi($"ERROR: {response}");
                }
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex);
                this.LogService.LogMachineAdminApi(ex.ToString());
            }

            return result;
        }

        public bool Charge(int amount, string chargingToken, ref string outMessage, string description = "")
        {
            var tryTime = 0;
            var result = false;

            try
            {
            Retry:
                tryTime++;
                var token = GetToken();
                this.LogService.LogMachineAdminApi($"Charge QR | amount = {amount} | Token: {chargingToken}");

                var client = new HttpClient
                {
                    BaseAddress = new Uri(_baseUrl)
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var paymentDetail = string.Empty;

                var charging = new ChargeWithTokenDto
                {
                    Amount = amount,
                    Token = chargingToken,
                    Description = description
                };

                var tDto = JsonConvert.SerializeObject(charging);
                this.LogService.LogMachineAdminApi("Charge QR | DTO = " + tDto);

                var response = client.PostAsJsonAsync($"api/services/app/Customer/ChargeWithToken", charging).Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsAsync<dynamic>().Result;
                    if (json.result != null)
                    {
                        var transactionId = json.result.transactionId;
                        var responseCode = json.result.responseCode;
                        var responseMessage = json.result.responseMessage;

                        if (responseCode == "00")
                        {
                            result = true;
                        }
                        outMessage = $"[{responseCode}] {responseMessage}";
                    }
                    LogService.LogMachineAdminApi($"Result: {json}");
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (tryTime <= 3)
                    {
                        Thread.Sleep(1000);
                        RenewToken();
                        goto Retry;
                    }
                    else
                    {
                        this.LogService.LogMachineAdminApi("Failed to get Token!");
                    }
                }
                else
                {
                    LogService.LogMachineAdminApi($"ERROR: {response}");
                }
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex);
                this.LogService.LogMachineAdminApi(ex.ToString());
            }

            return result;
        }

        public string RenewToken()
        {
            try
            {

                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(_baseUrl)
                };
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Abp.TenantId", _tenant);
                var login = "{\"userNameOrEmailAddress\": \"" + _userName + "\",\"password\": \"" + _password + "\"}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/api/TokenAuth/Authenticate")
                {
                    Content = new StringContent(login, Encoding.UTF8, "application/json")
                };

                dynamic res;
                var response = client.SendAsync(request).Result;
                using (var content = response.Content)
                {
                    var result = content.ReadAsStringAsync();
                    res = result.Result;
                }

                var returnData = string.IsNullOrEmpty(res) ? null : JObject.Parse(res);
                if (!string.IsNullOrEmpty(res))
                {
                    _token = returnData.result.accessToken;
                    return returnData.result.accessToken;
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }

            return string.Empty;
        }
    }
}
