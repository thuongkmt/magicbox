using Newtonsoft.Json;

namespace KonbiCloud.LinePay.Models
{
    /// <summary>
    /// Refund Payment Response
    /// </summary>
    public class RefundResponse : ResponseBase
    {
        /// <summary>
        /// Refund Information
        /// </summary>
        [JsonProperty("info")]
        public RefundInfo Info { get; set; }
    }
}
