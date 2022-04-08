using Abp.Application.Services;
using Abp.Configuration;
using Abp.UI;
using Castle.Core.Logging;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace KonbiCloud.CloudSync
{
    public class RestApiSessionSyncService : ApplicationService, ISessionSyncService, IApplicationService
    {
        private readonly ILogger _logger;
        private string serverUrl = null;
        public RestApiSessionSyncService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<Sessions.Session>> Sync(Guid machineId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                    var httpResponse = await httpClient.GetStringAsync($"{serverUrl}/api/services/app/Sessions/GetSessions?Id={machineId}");
                    var responseObject = JsonConvert.DeserializeObject<SyncApiResponse<Sessions.Session>>(httpResponse);
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
