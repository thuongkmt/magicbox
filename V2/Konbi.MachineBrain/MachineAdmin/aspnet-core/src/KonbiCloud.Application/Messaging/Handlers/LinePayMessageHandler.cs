using Castle.Core.Logging;
using Konbini.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KonbiCloud.Messaging.Handlers
{
    public class LinePayMessageHandler : ILinePayMessageHandler
    {
        private readonly ILogger _logger;
        public static Dictionary<string, Int64> _cache = new Dictionary<string, long>();

        public LinePayMessageHandler()
        {

        }

        public async Task<bool> Handle(KeyValueMessage keyValueMessage)
        {
            try
            {
                var linePay = JsonConvert.DeserializeObject<LinePayDto>(keyValueMessage.JsonValue);

                return true;
            }
            catch (Exception e)
            {
                _logger.Error("Error Handle LinePay: ", e);
                return false;
            }
        }
    }

    public class LinePayDto
    {
        public string regkey { get; set; }
        public Int64 transactionId { get; set; }
    }
}
