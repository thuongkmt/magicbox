using Abp.Application.Services;
using Abp.Configuration;
using Castle.Core.Logging;
using KonbiBrain.Common;
using KonbiCloud.Configuration;
using KonbiCloud.Transactions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Abp;

namespace KonbiCloud.CloudSync
{
    public class RestApiTransactionSyncService : AbpServiceBase, ITransactionSyncService
    {
        private readonly ILogger _logger;
        private string serverUrl = null;
        public RestApiTransactionSyncService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<RestApiGenericResult<long>> PushTransactionsToServer(IList<DetailTransaction> trans)
        {
            var result = new RestApiGenericResult<long>();
            if (SettingManager == null)
            {
                _logger?.Error($"Transaction Sync Service: SettingManager is null");
                return result;
            }
            try
            {
                using (var httpClient = new HttpClient())
                {
                    serverUrl = SettingManager.GetSettingValue(AppSettingNames.SyncServerUrl);
                    var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(trans));
                    var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                    var httpResponse = await httpClient.PostAsync($"{serverUrl}/api/services/app/Transaction/AddTransactions", httpContent);

                    if (httpResponse.Content != null && httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var responseContent = await httpResponse.Content.ReadAsStringAsync();
                        result = JsonConvert.DeserializeObject<RestApiGenericResult<long>>(responseContent);
                        return result;
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                _logger?.Error(e.Message);
                _logger?.Error(e.StackTrace);
                return result;
            }
        }
    }
}
