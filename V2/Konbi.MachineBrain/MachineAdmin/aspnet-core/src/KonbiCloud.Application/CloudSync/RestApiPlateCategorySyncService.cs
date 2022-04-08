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
    public class RestApiPlateCategorySyncService : ApplicationService, IPlateCategorySyncService, IApplicationService
    {
        private readonly ILogger _logger;
        private string serverUrl = null;
        public RestApiPlateCategorySyncService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<PlateCategory>> Sync(Guid mId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                    var httpResponse = await httpClient.GetStringAsync($"{serverUrl}/api/services/app/PlateCategories/GetCategories?Id={mId}");
                    var responseObject = JsonConvert.DeserializeObject<SyncApiResponse<PlateCategory>>(httpResponse);
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
    }
}
