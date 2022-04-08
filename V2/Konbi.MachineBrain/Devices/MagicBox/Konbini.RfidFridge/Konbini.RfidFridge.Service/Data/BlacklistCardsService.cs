using Konbini.RfidFridge.Domain;
using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.Entities;
using Konbini.RfidFridge.Domain.Enums;
using Konbini.RfidFridge.Service.Core;
using Konbini.RfidFridge.Service.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Data
{
    public class BlacklistCardsService : IBlacklistCardsService
    {
        private WebApiService WebApiService;
        private LogService LogService;
        public List<BlackListCardsDto> BlackListCards { get; set; }
        private bool IsEnable { get; set; }

        public BlacklistCardsService(WebApiService webApiService, LogService logService)
        {
            this.WebApiService = webApiService;
            this.LogService = logService;
        }


        public void Add(BlackListCardsDto card)
        {
            if (!IsEnable)
            {
                return;
            }
            this.LogService.LogInfo("ADDING BLACKLIST CARD");
            var tryTime = 0;
            if (string.IsNullOrEmpty(card.CardNumber))
            {
                this.LogService.LogInfo("Card Number is blank!!");
                return;
            }
            try
            {
            Retry:
                tryTime++;
                var token = WebApiService.GetToken();

                this.LogService.LogMachineAdminApi("Add Blacklistcard | Card Number = " + card.CardNumber);

                var client = new HttpClient
                {
                    BaseAddress = new Uri(WebApiService.GetUrl())
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var tDto = JsonConvert.SerializeObject(card);
                this.LogService.LogMachineAdminApi("Add Blacklistcard | DTO = " + tDto);

                var response = client.PostAsJsonAsync($"api/services/app/BlackListCards/Save", card).Result;

                if (response.IsSuccessStatusCode)
                {
                    var json = response.Content.ReadAsAsync<dynamic>().Result;
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
                this.LogService.LogMachineAdminApi(ex.ToString());
            }
            ReloadData();
        }

        public void Init(bool isEnable)
        {
            IsEnable = isEnable;
            if (IsEnable)
            {
                LogService.LogInfo("Init BlackList service");
                ReloadData();
            }

        }

        public void ReloadData()
        {

            BlackListCards = GetAllFromWebApi().Result;
            LogService.LogInfo($"Reload Blacklist, total {BlackListCards.Count} records");
        }

        public bool CheckCPASBlaskList(string cardNumber)
        {
            if (!IsEnable)
            {
                return false;
            }
            var result = false;
            result = BlackListCards.Any(x => x.CardNumber.Equals(cardNumber, StringComparison.OrdinalIgnoreCase));
            LogService.LogInfo($"Checking blacklist card: {cardNumber} | result: {result}");

            return result;
        }

        private async Task<List<BlackListCardsDto>> GetAllFromWebApi()
        {
            var tryTime = 0;
            var data = new List<BlackListCardsDto>();
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
                var response = await client.GetAsync($"api/services/app/BlackListCards/GetAllItems");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsAsync<dynamic>();
                    var dataList = JsonConvert.SerializeObject(json.result);
                    data = JsonConvert.DeserializeObject<List<BlackListCardsDto>>(dataList);
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
            catch (HttpRequestException)
            {
                this.LogService.LogError("API Server is not started!.");
            }
            catch (Exception ex)
            {
                this.LogService.LogError(ex);
                return null;
            }
            return data;
        }
    }
}
