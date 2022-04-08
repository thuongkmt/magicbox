using Newtonsoft.Json;

namespace LinePayCSharp
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
