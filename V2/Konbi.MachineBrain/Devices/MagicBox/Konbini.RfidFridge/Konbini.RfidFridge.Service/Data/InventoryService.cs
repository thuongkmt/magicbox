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
    using Konbini.RfidFridge.Domain.DTO;

    public class InventoryService : EntityService<Inventory>, IInventoryService
    {
        private WebApiService WebApiService;
        private LogService LogService;

        public InventoryService(WebApiService webApiService, LogService logService)
        {
            this.WebApiService = webApiService;
            this.LogService = logService;
        }

        public async Task Delete(string id)
        {
            var tryTime = 0;

            try
            {
            Retry:
                tryTime++;
                var token = WebApiService.GetToken();

                //LogService.LogInfo($"Token: {token}");
                LogService.LogMachineAdminApi($"Inventory Service | Delete");
                LogService.LogMachineAdminApi($"Trying time: {tryTime}");

                var client = new HttpClient
                {
                    BaseAddress = new Uri(WebApiService.GetUrl())
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var response = await client.PostAsJsonAsync($"api/services/app/Inventories/Delete", id);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsAsync<dynamic>();
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
                this.LogService.LogMachineAdminApi("API Server is not started!.");
            }
            catch (Exception ex)
            {
                this.LogService.LogMachineAdminApi(ex.ToString());
            }
        }

        public async Task<List<InventoryDto>> GetAllFromWebApi()
        {
            var tryTime = 0;
            var data = new List<InventoryDto>();
            try
            {
            Retry:
                tryTime++;
                var token = WebApiService.GetToken();

                //LogService.LogInfo($"Token: {token}");
                LogService.LogInfo($"Trying time: {tryTime}");

                var client = new HttpClient
                {
                    BaseAddress = new Uri(WebApiService.GetUrl())
                };
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                var response = await client.GetAsync($"api/services/app/Inventories/GetAllItems");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsAsync<dynamic>();
                    foreach (var r in json.result)
                    {
                        var inventory = r.inventory;
                        var product = r.product;
                        int.TryParse(Convert.ToString(inventory.trayLevel), out int traylevel);
                        double.TryParse(Convert.ToString(inventory.price), out double outPrice);
                        double.TryParse(Convert.ToString(product.price), out double poutPrice);
                        data.Add(new InventoryDto
                        {
                            TagId = inventory.tagId,
                            TrayLevel = traylevel,
                            Price = outPrice,
                            Id = inventory.id,
                            Product = new ProductDto
                            {
                                Id = inventory.productId,
                                ProductName = product.name,
                                SKU = product.sku,
                                Price = poutPrice
                            }
                        });
                    }

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
                    }
                }
                else
                {
                    LogService.LogInfo($"ERROR: {response}");
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

        public void RemoveByIds(List<string> ids)
        {
            Task.Run(() =>
            {
                foreach (var id in ids)
                {
                    Delete(id).Wait();
                }
            });
        }
    }
}
