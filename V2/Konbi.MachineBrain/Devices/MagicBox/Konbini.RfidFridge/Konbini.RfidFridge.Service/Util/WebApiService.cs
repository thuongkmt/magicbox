using Konbini.RfidFridge.Domain.Entities;
using Konbini.RfidFridge.Service.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Konbini.RfidFridge.Service.Util
{

    public class WebApiService
    {
        private string _baseUrl;
        private string _userName;
        private string _password;
        private string _token;

        private LogService _logService;
        public WebApiService(LogService logService)
        {
            _logService = logService;
        }

        public void Init(string baseUrl, string userName, string password)
        {
            this._baseUrl = baseUrl;
            this._userName = userName;
            this._password = password;

            //RenewToken();
        }

        public string GetToken()
        {
            return this._token;
        }

        public string GetUrl()
        {
            return this._baseUrl;
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

        public string RenewToken()
        {
            try
            {
                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(_baseUrl)
                };
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Abp.TenantId", "1");
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
                _logService.LogError(ex);
            }

            return string.Empty;
        }
    }
}
