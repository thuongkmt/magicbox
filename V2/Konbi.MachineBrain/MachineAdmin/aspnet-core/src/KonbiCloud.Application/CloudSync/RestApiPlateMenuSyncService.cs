using Abp.Application.Services;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.PlateMenus.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public class RestApiPlateMenuSyncService : ApplicationService, IPlateMenuSyncService, IApplicationService
    {
        private readonly ILogger _logger;
        private string serverUrl = null;
        public RestApiPlateMenuSyncService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<MenuSchedule.PlateMenu>> Sync(Guid machineId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                    var httpResponse = await httpClient.GetStringAsync($"{serverUrl}/api/services/app/PlateMenus/GetPlateMenus?Id={machineId}");
                    var responseObject = JsonConvert.DeserializeObject<SyncApiResponse<MenuSchedule.PlateMenu>>(httpResponse);
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

                    var httpResponse = await httpClient.PutAsync($"{serverUrl}/api/services/app/PlateMenus/UpdateSyncStatus", httpContent);
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
