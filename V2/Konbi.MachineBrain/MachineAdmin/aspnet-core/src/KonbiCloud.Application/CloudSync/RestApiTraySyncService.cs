using Abp.Application.Services;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiBrain.Common;
using KonbiCloud.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Machines;
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
    public class RestApiTraySyncService : ApplicationService, ITraySyncService, IApplicationService
    {
        private readonly ILogger _logger;
        private string serverUrl = null;
        public RestApiTraySyncService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<List<Plate.Tray>> Sync(Guid machineId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                    var httpResponse = await httpClient.GetStringAsync($"{serverUrl}/api/services/app/Tray/GetTrays?Id={machineId}");
                    var trayResponse = JsonConvert.DeserializeObject< SyncApiResponse<Plate.Tray>>(httpResponse);
                    return trayResponse.result;
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
