using Konbini.RfidFridge.Domain.Entities;
using Konbini.RfidFridge.Service.Base;
using Konbini.RfidFridge.Service.Core;
using Konbini.RfidFridge.Service.Util;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Data
{
    using Konbini.RfidFridge.Common;
    using Konbini.RfidFridge.Domain.DTO;
    using System.ComponentModel;
    using System.Reflection;

    public class SettingService : ISettingService
    {
        private WebApiService WebApiService;

        private LogService LogService;

        public SettingService(WebApiService webApiService, LogService logService)
        {
            this.WebApiService = webApiService;
            this.LogService = logService;
        }
        public async Task<bool> GetAll()
        {
            var tryTime = 0;
            var data = new List<InventoryDto>();
            try
            {
                Retry:
                tryTime++;
                var token = WebApiService.GetToken();

                var client = new HttpClient
                {
                    BaseAddress = new Uri(WebApiService.GetUrl())
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var response = await client.GetAsync($"api/services/app/SystemConfigService/GetAll");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsAsync<dynamic>();
                    var settingDict = new Dictionary<string, string>();
                    foreach (var setting in json.result.items)
                    {
                        if (!settingDict.ContainsKey(Convert.ToString(setting.name)))
                        {
                            settingDict.Add(Convert.ToString(setting.name), Convert.ToString(setting.value));
                        }
                    }


                    var settings = Assembly
                        .GetAssembly(typeof(RfidFridgeSetting))
                        .GetTypes();


                    foreach (var setting in settings)
                    {
                        var properties = setting.GetProperties();
                        if (properties.Length > 0)
                        {

                            var settingKey = setting.FullName.Replace(setting.Namespace, string.Empty);
                            settingKey = settingKey.Substring(1, settingKey.Length - 1).Replace("+", ".");

                            foreach (var propertyInfo in properties)
                            {
                                var key = $"{settingKey}.{propertyInfo.Name}";

                                if (settingDict.ContainsKey(key))
                                {
                                    var val = settingDict[key];
                                    propertyInfo.SetValue(setting, val);
                                }
                            }
                        }

                    }
                    return true;
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (tryTime <= 3)
                    {
                        Thread.Sleep(1000);
                        this.WebApiService.RenewToken();
                        goto Retry;
                    }
                    else
                    {
                        this.LogService.LogInfo("Failed to get Token!");
                        return false;
                    }
                }
                else
                {
                    LogService.LogInfo($"ERROR: {response}");
                    return false;
                }
            }
            catch (HttpRequestException)
            {
                this.LogService.LogError("API Server is not started!.");
                return false;
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex);
                return false;
            }
        }
    }
}
