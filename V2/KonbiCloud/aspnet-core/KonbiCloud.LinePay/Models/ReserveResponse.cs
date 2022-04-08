using Newtonsoft.Json;

namespace KonbiCloud.LinePay.Models
{
    /// <summary>
    /// Payment Reserve Response
    /// </summary>
    public class ReserveResponse : ResponseBase
    {
        /// <summary>
        /// Reserve Information
        /// </summary>
        [JsonProperty("info")]
        public ReserveInfo Info { get; set; }
    }
}
