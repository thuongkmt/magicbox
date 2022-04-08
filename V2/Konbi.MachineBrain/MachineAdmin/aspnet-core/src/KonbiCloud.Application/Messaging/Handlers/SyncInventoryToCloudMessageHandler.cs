using Castle.Core.Logging;
using Konbini.Messages;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging.Handlers
{
    public class SyncInventoryToCloudMessageHandler : ISyncInventoryToCloudMessageHandler
    {
        private readonly ILogger _logger;
        private string BASE_URL { get; set; }

        public SyncInventoryToCloudMessageHandler(
            ILogger logger)
        {
            BASE_URL = "http://localhost:9000"; ;
            _logger = logger;
        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                var client = new HttpClient { BaseAddress = new Uri(BASE_URL) };

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.PostAsync("/api/machine/inventory/reload",null);

                using (var content = response.Content)
                {
                    _logger.Info("Sending inventory message to cloud result : " + content);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Error when synchronize inventories from machine to cloud", ex);
                return false;
            }
        }
    }
}
