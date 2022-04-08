using Newtonsoft.Json;

namespace KonbiCloud.LinePay.Models
{
    /// <summary>
    /// Payment ConfirmAPI Response
    /// </summary>
    public class ConfirmResponse : ResponseBase
    {
        [JsonProperty("info")]
        public ConfirmInfo Info { get; set; }
    }
}
