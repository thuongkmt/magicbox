using Newtonsoft.Json;

namespace KonbiCloud.LinePay.Models
{
    /// <summary>
    /// Refund
    /// </summary>
    public class Refund
    {
        [JsonProperty("refundAmount")]
        public int RefundAmount { get; set; }
    }
}
