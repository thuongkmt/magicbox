using Konbini.RfidFridge.TagManagement.Data;
using Konbini.RfidFridge.TagManagement.DTO;
using Konbini.RfidFridge.TagManagement.Enums;
using Konbini.RfidFridge.TagManagement.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static Konbini.RfidFridge.TagManagement.DTO.ProductDTO;

namespace Konbini.RfidFridge.TagManagement.Service
{
    public class MbCloudService : IMbCloudService
    {
        private string BASE_URL { get; set; }
        private string USER_NAME { get; set; }
        private string PASSWORD { get; set; }
        private string TENANT_ID { get; set; }


        public MbCloudService()
        {
            try
            {
                using (var context = new KDbContext())
                {
                    BASE_URL = context.Settings.SingleOrDefault(x => x.Key == SettingKey.CloudUrl)?.Value;
                    USER_NAME = context.Settings.SingleOrDefault(x => x.Key == SettingKey.UserName)?.Value;
                    PASSWORD = context.Settings.SingleOrDefault(x => x.Key == SettingKey.Password)?.Value;
                    TENANT_ID = context.Settings.SingleOrDefault(x => x.Key == SettingKey.TenantId)?.Value;
                }
            }
            catch (Exception ex)
            {
                SeriLogService.LogError(ex.ToString());
            }
        }
        public List<MachineDTO.Machine> GetAllMachines()
        {
            var data = GetAsync("/api/services/app/Machine/GetAll?MaxResultCount=10000&SkipCount=0");
            MachineDTO.Data returnData = JsonConvert.DeserializeObject<MachineDTO.Data>(Convert.ToString(data));
            return returnData.Result.Items;
        }
        public List<ProductDTO.Product> GetAllProducts()
        {
            var data = GetAsync("/api/services/app/Products/GetAll?MaxResultCount=10000&SkipCount=0");
            var s = Convert.ToString(data);
            ProductDTO.Data returnData = JsonConvert.DeserializeObject<ProductDTO.Data>(Convert.ToString(data));
            var a = JsonConvert.DeserializeObject<RootObject>(Convert.ToString(data));
            var list = returnData.Result.Items.Select(x => x).ToList();
            return list;
        }

        public async Task<bool> BulkInsertTags(BulkTagsDto input)
        {
            using (var httpClient = new HttpClient())
            {
                var url = new Uri(BASE_URL);
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), url.AbsoluteUri + "/api/services/app/ProductTags/InsertTags"))
                {
                    request.Headers.TryAddWithoutValidation("accept", "application/json");
                    request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + GetToken());

                    var json = JsonConvert.SerializeObject(input);

                    request.Content = new StringContent(json);
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json-patch+json");

                    var response = await httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        SeriLogService.LogInfo("BulkInsertTags" + response.ToString());
                    }
                }
            }
            return false;
        }


        public class Item
        {
            public string name { get; set; }
            public string sku { get; set; }
            public object barcode { get; set; }
            public object shortDesc { get; set; }
            public object desc { get; set; }
            public object tag { get; set; }
            public object imageUrl { get; set; }
            public double price { get; set; }
            public DateTime creationTime { get; set; }
            public List<object> categoriesName { get; set; }
            public string id { get; set; }
        }

        public class Result
        {
            public int totalCount { get; set; }
            public List<Item> items { get; set; }
        }

        public class RootObject
        {
            public Result result { get; set; }
            public object targetUrl { get; set; }
            public bool success { get; set; }
            public object error { get; set; }
            public bool unAuthorizedRequest { get; set; }
            public bool __abp { get; set; }
        }

        private dynamic GetAsync(string api)
        {
            var client = new HttpClient { BaseAddress = new Uri(BASE_URL) };
            var token = GetToken();

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            var response = client.GetAsync(api).Result;
            dynamic res;
            using (var content = response.Content)
            {
                var result = content.ReadAsStringAsync();
                res = result.Result;
            }
            return string.IsNullOrEmpty(res) ? null : JObject.Parse(res);
        }

        private string GetStringAsync(string api)
        {
            var client = new HttpClient { BaseAddress = new Uri(BASE_URL) };
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

        public string GetToken()
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(BASE_URL)
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (TENANT_ID != "0")
            {
                client.DefaultRequestHeaders.Add("Abp.TenantId", TENANT_ID);
            }
            var login = "{\"userNameOrEmailAddress\": \"" + USER_NAME + "\",\"password\": \"" + PASSWORD + "\"}";
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
                return returnData.result.accessToken;
            }
            return string.Empty;
        }

    }




}
