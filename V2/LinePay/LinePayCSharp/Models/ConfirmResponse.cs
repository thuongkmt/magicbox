using Newtonsoft.Json;

namespace LinePayCSharp
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
