using Abp.Application.Services;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiBrain.Common;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Plate;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public class RestApiDishSyncService : ApplicationService, IDishSyncService, IApplicationService
    {
        private readonly ILogger _logger;
        private string serverUrl = null;
        public RestApiDishSyncService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<bool> PushToServer(SyncedItemData<Disc> dishes)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                    var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(dishes));
                    var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                    var httpResponse = await httpClient.PostAsync($"{serverUrl}/api/services/app/Discs/SyncDishData", httpContent);

                    // If the response contains content we want to read it!
                    if (httpResponse.Content != null && httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var responseContent = await httpResponse.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<RestApiResult>(responseContent);
                        if (result.success && result.result)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                _logger?.Error(e.Message);
                _logger?.Error(e.StackTrace);
                return false;
            }
        }

        public async Task<bool> Sync(Disc entity)
        {
            return false;
        }

        public async Task<List<Disc>> SyncFromServer(Guid machineId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                    var httpResponse = await httpClient.GetStringAsync($"{serverUrl}/api/services/app/Discs/GetDishes?Id={machineId}");
                    var responseObject = JsonConvert.DeserializeObject<SyncApiResponse<Disc>>(httpResponse);
                    return responseObject.result;
                }
            }
            catch (Exception e)
            {
                _logger?.Error(e.Message);
                _logger?.Error(e.StackTrace);
                return null;
            }
        }

        public async Task<bool> UpdateSyncStatus(SyncedItemData<Guid> input)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                    string json = JsonConvert.SerializeObject(input, Formatting.Indented);
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                    var httpResponse = await httpClient.PutAsync($"{serverUrl}/api/services/app/Discs/UpdateSyncStatus", httpContent);
                    return httpResponse.IsSuccessStatusCode;
                }
            }
            catch (Exception e)
            {
                _logger?.Error(e.Message);
                _logger?.Error(e.StackTrace);
                return false;
            }
        }
    }
}
