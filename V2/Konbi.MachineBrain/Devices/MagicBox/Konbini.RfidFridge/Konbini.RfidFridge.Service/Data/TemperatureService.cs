using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.Entities;
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
    public class TemperatureService : ITemperatureService
    {
        private WebApiService WebApiService;
        private LogService LogService;

        public TemperatureService(WebApiService webApiService, LogService logService)
        {
            this.WebApiService = webApiService;
            this.LogService = logService;
        }
        public void Log(double temp)
        {
            this.LogService.LogMachineAdminApi("==================LOG TEMP===================");
            var tryTime = 0;

            try
            {
                Retry:
                tryTime++;
                var token = WebApiService.GetToken();

                LogService.LogMachineAdminApi($"Token: {token}");
                LogService.LogMachineAdminApi($"Trying time: {tryTime}");

                var client = new HttpClient
                {
                    BaseAddress = new Uri(WebApiService.GetUrl())
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var response =  client.PostAsync($"/api/services/app/TemperatureLogs/AddTemperatureLog?temperatureLog={temp}", null).Result;

                if (response.IsSuccessStatusCode)
                {
                    var json =  response.Content.ReadAsAsync<dynamic>().Result;
                    LogService.LogMachineAdminApi($"Result: {json}");
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
            }
        }
    }
}
